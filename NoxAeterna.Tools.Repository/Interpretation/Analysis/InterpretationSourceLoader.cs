using System.Security.Cryptography;
using System.Text;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Serialization;
using NoxAeterna.Interpretation.Tarot.Validation;
using NoxAeterna.Tools.Repository.Interpretation.Reports;

namespace NoxAeterna.Tools.Repository.Interpretation.Analysis;

public sealed record InterpretationSourceCompilationResult(
    InterpretationToolReport Report,
    TarotInterpretationCompilation? Compilation);

/// <summary>Strictly validates canonical authoring JSON and builds the normalized compiler input.</summary>
public sealed class InterpretationSourceLoader
{
    private static readonly TarotDeckDefinition Deck = StandardTarotCatalog.Deck;

    public InterpretationSourceCompilationResult Load(string sourceRoot)
    {
        var paths = new InterpretationPackPaths(sourceRoot, mustExist: true);
        var bag = new InterpretationDiagnosticBag();
        var files = Directory.EnumerateFiles(paths.Root, "*.json", SearchOption.AllDirectories)
            .Select(path => (Absolute: path, Relative: paths.Relative(path)))
            .OrderBy(static item => item.Relative, StringComparer.Ordinal).ToArray();
        foreach (var file in files) { try { paths.Resolve(file.Relative); } catch (ArgumentException e) { bag.Error("source.path", file.Relative, e.Message); } }
        var manifestFile = files.SingleOrDefault(static item => item.Relative == "interpretation-pack.json");
        if (manifestFile == default) return Result(bag, null, files, new Dictionary<string,int>(), "source.manifest-missing", "interpretation-pack.json", "Source manifest is missing.");
        var manifest = Read(manifestFile, static bytes => TarotInterpretationJson.Parse<TarotInterpretationPackDocument>(bytes), TarotInterpretationJson.Serialize, TarotInterpretationValidator.ValidateManifest, bag);
        if (manifest is null) return Result(bag, null, files, new Dictionary<string,int>());

        var labels = Init<TarotLabels>(manifest);
        var vocabulary = InitList<TarotVocabularyEntry>(manifest);
        var singles = InitList<TarotSingleCardEntry>(manifest);
        var pairs = InitList<TarotOrientedPairEntry>(manifest);
        var positions = InitList<TarotThreeCardPositionEntry>(manifest);
        var synthesis = InitList<TarotSynthesisResource>(manifest);
        var identities = new HashSet<string>(StringComparer.Ordinal);
        var validBundles = 0; var invalidBundles = 0; var noncanonical = 0; var duplicates = 0;

        foreach (var file in files.Where(static item => item.Relative != "interpretation-pack.json"))
        {
            var segments = file.Relative.Split('/');
            if (segments.Length < 3 || segments[0] != "content" || !TryLocale(manifest, segments[1], out var locale))
            {
                bag.Error("source.taxonomy", file.Relative, "JSON file is outside the canonical content/<locale> taxonomy."); invalidBundles++; continue;
            }
            var kind = segments[2];
            if (kind == "labels.json" && segments.Length == 3)
            {
                var value = Read(file, static bytes => TarotInterpretationJson.Parse<TarotLabelsDocument>(bytes), TarotInterpretationJson.Serialize, TarotInterpretationValidator.ValidateLabels, bag);
                if (value is null) invalidBundles++; else { labels[locale] = value; validBundles++; }
                continue;
            }
            if (kind == "vocabulary" && segments.Length == 4 && segments[3].EndsWith(".json", StringComparison.Ordinal))
            {
                var value = Read(file, static bytes => TarotInterpretationJson.Parse<TarotVocabularyDocument>(bytes), TarotInterpretationJson.Serialize, TarotInterpretationValidator.ValidateVocabulary, bag);
                if (value is null) { invalidBundles++; continue; }
                var expected = segments[3][..^5];
                var identity = identities.Add($"v|{locale.Value}|{value.ConceptId.Value}");
                if (!identity) { bag.Error("source.duplicate", file.Relative, "Duplicate vocabulary identity."); duplicates++; }
                if (value.ConceptId.Value != expected) { bag.Error("source.identity-path", file.Relative, "conceptId does not match filename."); noncanonical++; }
                if (!identity || value.ConceptId.Value != expected) { invalidBundles++; continue; }
                vocabulary[locale].Add(value); validBundles++; continue;
            }
            if (kind == "single-card" && segments.Length == 4 && segments[3].EndsWith(".json", StringComparison.Ordinal))
            {
                var value = Read(file, static bytes => TarotInterpretationJson.Parse<TarotSingleCardBundleDocument>(bytes), TarotInterpretationJson.Serialize, d => TarotInterpretationValidator.ValidateSingleCardBundle(d, Deck), bag);
                if (value is null) { invalidBundles++; continue; }
                var card = value[0].CardId.Value; var expected = segments[3][..^5];
                var identity = identities.Add($"s|{locale.Value}|{card}");
                if (!identity) { bag.Error("source.duplicate", file.Relative, "Duplicate single-card bundle identity."); duplicates++; }
                if (card != expected) { bag.Error("source.identity-path", file.Relative, "cardId does not match filename."); noncanonical++; }
                if (!identity || card != expected) { invalidBundles++; continue; }
                singles[locale].AddRange(value); validBundles++; continue;
            }
            if (kind == "oriented-pairs" && segments.Length == 4 && segments[3].EndsWith(".json", StringComparison.Ordinal))
            {
                var value = Read(file, static bytes => TarotInterpretationJson.Parse<TarotOrientedPairBundleDocument>(bytes), TarotInterpretationJson.Serialize, d => TarotInterpretationValidator.ValidateOrientedPairBundle(d, Deck), bag);
                if (value is null) { invalidBundles++; continue; }
                var pair = $"{value[0].CardAId.Value}__{value[0].CardBId.Value}"; var expected = segments[3][..^5];
                var identity = identities.Add($"p|{locale.Value}|{pair}");
                if (!identity) { bag.Error("source.duplicate", file.Relative, "Duplicate pair bundle identity."); duplicates++; }
                if (pair != expected) { bag.Error("source.identity-path", file.Relative, "Pair identity does not match filename."); noncanonical++; }
                if (!identity || pair != expected) { invalidBundles++; continue; }
                pairs[locale].AddRange(value); validBundles++; continue;
            }
            if (kind == "three-card-positions" && segments.Length == 4 && segments[3].EndsWith(".json", StringComparison.Ordinal))
            {
                var value = Read(file, static bytes => TarotInterpretationJson.Parse<TarotThreeCardPositionsBundleDocument>(bytes), TarotInterpretationJson.Serialize, d => TarotInterpretationValidator.ValidateThreeCardPositionsBundle(d, Deck), bag);
                if (value is null) { invalidBundles++; continue; }
                var card = value[0].CardId.Value; var expected = segments[3][..^5];
                var identity = identities.Add($"t|{locale.Value}|{card}");
                if (!identity) { bag.Error("source.duplicate", file.Relative, "Duplicate three-card-position bundle identity."); duplicates++; }
                if (card != expected) { bag.Error("source.identity-path", file.Relative, "cardId does not match filename."); noncanonical++; }
                if (!identity || card != expected) { invalidBundles++; continue; }
                positions[locale].AddRange(value); validBundles++; continue;
            }
            if (kind == "synthesis" && segments.Length == 5 && segments[4].EndsWith(".json", StringComparison.Ordinal))
            {
                var parsed = ParseCanonical<TarotSynthesisResourceDocument>(file, bag);
                if (parsed is null || parsed.SchemaVersion != 1 || parsed.ResourceType is null || parsed.ResourceId is null || parsed.Data is null)
                { bag.Error("source.synthesis", file.Relative, "Synthesis resource envelope is incomplete."); invalidBundles++; continue; }
                var type = TarotSchemaText.Get(parsed.ResourceType.Value, TarotSchemaText.SynthesisResourceTypes); var id = ParseId(parsed.ResourceId, file.Relative, bag);
                if (id is null || type != segments[3] || id.Value != segments[4][..^5]) { bag.Error("source.identity-path", file.Relative, "Synthesis identity does not match path."); noncanonical++; invalidBundles++; continue; }
                if (!identities.Add($"y|{locale.Value}|{type}|{id.Value}")) { bag.Error("source.duplicate", file.Relative, "Duplicate synthesis identity."); duplicates++; invalidBundles++; continue; }
                synthesis[locale].Add(new(parsed.ResourceType.Value,id,Encoding.UTF8.GetString(TarotInterpretationJson.Serialize(parsed.Data.Value)))); validBundles++; continue;
            }
            bag.Error("source.taxonomy", file.Relative, "JSON file does not match a canonical source path."); noncanonical++; invalidBundles++;
        }

        foreach (var locale in manifest.DeclaredLocales)
        {
            if (labels[locale] is null) bag.Error("source.labels-missing", $"content/{locale.Value}/labels.json", "Every declared locale requires labels.json.");
            var concepts = vocabulary[locale].Select(static item => item.ConceptId).ToHashSet();
            foreach (var tag in singles[locale].SelectMany(static item => item.Tags).Concat(pairs[locale].SelectMany(static item => item.Tags)).Concat(positions[locale].SelectMany(static item => item.Tags)))
                if (!concepts.Contains(tag.ConceptId)) bag.Error("source.vocabulary-missing", $"content/{locale.Value}/vocabulary/{tag.ConceptId.Value}.json", "Every used tag requires a same-locale vocabulary entry.");
            ValidateReadiness(manifest, locale, singles[locale], pairs[locale], positions[locale], synthesis[locale], bag);
        }

        var cardIds = Deck.Cards.Select(static card => card.Id.Value).ToHashSet(StringComparer.Ordinal);
        var pairIdentities = Deck.Cards.Select(static card => card.Id.Value).SelectMany((a,i) => Deck.Cards.Skip(i+1).Select(b => $"{a}__{b.Id.Value}")).ToHashSet(StringComparer.Ordinal);
        var counts = new Dictionary<string,int>(StringComparer.Ordinal)
        {
            ["sourceFiles"] = files.Length, ["validBundles"] = validBundles, ["invalidBundles"] = invalidBundles,
            ["duplicateIdentities"] = duplicates, ["noncanonicalIdentities"] = noncanonical,
            ["singleCardBundles"] = singles.Sum(static pair => pair.Value.Count / 2), ["singleCardStates"] = singles.Sum(static pair => pair.Value.Count),
            ["missingSingleCardBundles"] = manifest.DeclaredLocales.Sum(locale => cardIds.Count - singles[locale].Select(static item => item.CardId.Value).Distinct().Count()),
            ["orientedPairBundles"] = pairs.Sum(static pair => pair.Value.Count / 4), ["orientedPairStates"] = pairs.Sum(static pair => pair.Value.Count),
            ["missingOrientedPairBundles"] = manifest.DeclaredLocales.Sum(locale => pairIdentities.Count - pairs[locale].Select(static item => $"{item.CardAId.Value}__{item.CardBId.Value}").Distinct().Count()),
            ["threeCardPositionBundles"] = positions.Sum(static pair => pair.Value.Count / 6), ["threeCardPositionStates"] = positions.Sum(static pair => pair.Value.Count),
            ["missingThreeCardPositionBundles"] = manifest.DeclaredLocales.Sum(locale => cardIds.Count - positions[locale].Select(static item => item.CardId.Value).Distinct().Count())
        };
        var compilation = bag.HasErrors ? null : new TarotInterpretationCompilation(
            manifest, new TarotSha256(SourceDigest(files)),
            labels.ToDictionary(static p=>p.Key,static p=>p.Value!),
            Freeze(vocabulary),Freeze(singles),Freeze(pairs),Freeze(positions),Freeze(synthesis));
        return Result(bag, compilation, files, counts);
    }

    private static TValue? Read<TDocument,TValue>((string Absolute,string Relative) file,Func<byte[],TarotJsonParseResult<TDocument>> parser,Func<TDocument,byte[]> serializer,Func<TDocument,TarotValidationResult<TValue>> validator,InterpretationDiagnosticBag bag) where TDocument:class where TValue:class
    {var parsed=ParseCanonical(file,parser,serializer,bag);if(parsed is null)return null;var validated=validator(parsed);bag.AddValidation(file.Relative,validated.Diagnostics);return validated.Value;}
    private static T? ParseCanonical<T>((string Absolute,string Relative) file,InterpretationDiagnosticBag bag) where T:class => ParseCanonical(file,static bytes=>TarotInterpretationJson.Parse<T>(bytes),TarotInterpretationJson.Serialize,bag);
    private static T? ParseCanonical<T>((string Absolute,string Relative) file,Func<byte[],TarotJsonParseResult<T>> parser,Func<T,byte[]> serializer,InterpretationDiagnosticBag bag) where T:class
    {var bytes=File.ReadAllBytes(file.Absolute);var parsed=parser(bytes);if(!parsed.IsSuccess||parsed.Document is null){bag.Error("source.json",file.Relative,parsed.Failure?.Message??"Malformed JSON.");return null;}if(!bytes.SequenceEqual(serializer(parsed.Document)))bag.Error("source.canonical-bytes",file.Relative,"Source JSON must exactly match canonical UTF-8 LF bytes.");return parsed.Document;}
    private static string SourceDigest(IEnumerable<(string Absolute,string Relative)> files){using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);foreach(var f in files.OrderBy(static f=>f.Relative,StringComparer.Ordinal)){hash.AppendData(Encoding.UTF8.GetBytes(f.Relative));hash.AppendData([0]);hash.AppendData(File.ReadAllBytes(f.Absolute));hash.AppendData([0]);}return Convert.ToHexStringLower(hash.GetHashAndReset());}
    private static Dictionary<TarotInterpretationLocale,T?> Init<T>(TarotInterpretationPackManifest m) where T:class=>m.DeclaredLocales.ToDictionary(static l=>l,static _=>(T?)null);
    private static Dictionary<TarotInterpretationLocale,List<T>> InitList<T>(TarotInterpretationPackManifest m)=>m.DeclaredLocales.ToDictionary(static l=>l,static _=>new List<T>());
    private static IReadOnlyDictionary<TarotInterpretationLocale,IReadOnlyList<T>> Freeze<T>(Dictionary<TarotInterpretationLocale,List<T>> x)=>x.ToDictionary(static p=>p.Key,static p=>(IReadOnlyList<T>)p.Value.AsReadOnly());
    private static bool TryLocale(TarotInterpretationPackManifest m,string raw,out TarotInterpretationLocale locale){locale=m.DeclaredLocales.FirstOrDefault(l=>l.Value==raw)!;return locale is not null;}
    private static TarotSynthesisResourceId? ParseId(string raw,string path,InterpretationDiagnosticBag bag){try{return new(raw);}catch(ArgumentException e){bag.Error("source.resource-id",path,e.Message);return null;}}
    private static void ValidateReadiness(TarotInterpretationPackManifest m,TarotInterpretationLocale l,List<TarotSingleCardEntry> s,List<TarotOrientedPairEntry> p,List<TarotThreeCardPositionEntry> t,List<TarotSynthesisResource> y,InterpretationDiagnosticBag bag){var modules=m.Modules.ToDictionary(static q=>q.Key,q=>q.Value[l].Ready);if(modules[TarotInterpretationMode.SingleCard]&&s.Count!=156)bag.Error("ready.single-card",l.Value,"A ready single-card module requires 78 bundles / 156 states.");if((modules[TarotInterpretationMode.TwoCards]||modules[TarotInterpretationMode.ThreeCards])&&p.Count!=12012)bag.Error("ready.oriented-pairs",l.Value,"A ready dependent module requires 3003 pair bundles / 12012 states.");if(modules[TarotInterpretationMode.ThreeCards]&&t.Count!=468)bag.Error("ready.positions",l.Value,"A ready three-cards module requires 78 position bundles / 468 states.");if(modules[TarotInterpretationMode.ThreeCards]&&y.Count==0)bag.Error("ready.synthesis",l.Value,"A ready three-cards module requires synthesis resources.");}
    private static InterpretationSourceCompilationResult Result(InterpretationDiagnosticBag bag,TarotInterpretationCompilation? c,(string Absolute,string Relative)[] files,Dictionary<string,int> counts,string? code=null,string? target=null,string? message=null){if(code is not null)bag.Error(code,target!,message!);return new(new InterpretationToolReport(bag.Items,counts),c);}
}

public sealed class InterpretationPackValidator
{
    public InterpretationToolReport Validate(string sourceRoot) => new InterpretationSourceLoader().Load(sourceRoot).Report;
}
