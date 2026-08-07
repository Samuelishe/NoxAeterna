namespace NoxAeterna.Interpretation.Sqlite;

/// <summary>Owns the immutable .noxinterp SQLite schema.</summary>
public static class TarotSqliteSchema
{
    public const int UserVersion = 1;
    public const int ApplicationId = 0x4E4F5849; // ASCII "NOXI".

    public const string Ddl = """
        PRAGMA foreign_keys = ON;
        CREATE TABLE pack_metadata(
          singleton INTEGER PRIMARY KEY CHECK(singleton = 1),
          package_schema_version INTEGER NOT NULL CHECK(package_schema_version = 1),
          pack_id TEXT NOT NULL,
          semantic_deck_id TEXT NOT NULL,
          source_locale TEXT NOT NULL,
          content_version INTEGER NOT NULL CHECK(content_version > 0),
          source_digest TEXT NOT NULL CHECK(length(source_digest) = 64 AND source_digest NOT GLOB '*[^0-9a-f]*')
        ) STRICT;
        CREATE TABLE declared_locale(
          locale TEXT PRIMARY KEY
        ) STRICT;
        CREATE TABLE display_name(
          locale TEXT PRIMARY KEY REFERENCES declared_locale(locale),
          value TEXT NOT NULL CHECK(length(trim(value)) > 0)
        ) STRICT;
        CREATE TABLE module(
          mode TEXT NOT NULL CHECK(mode IN ('single-card','two-cards','three-cards','celtic-cross')),
          locale TEXT NOT NULL REFERENCES declared_locale(locale),
          ready INTEGER NOT NULL CHECK(ready IN (0,1)),
          PRIMARY KEY(mode, locale)
        ) STRICT;
        CREATE TABLE module_dependency(
          mode TEXT NOT NULL,
          locale TEXT NOT NULL,
          ordinal INTEGER NOT NULL CHECK(ordinal >= 0),
          dependency TEXT NOT NULL CHECK(dependency IN ('oriented-pairs','three-card-positions','three-card-synthesis')),
          PRIMARY KEY(mode, locale, ordinal),
          UNIQUE(mode, locale, dependency),
          FOREIGN KEY(mode, locale) REFERENCES module(mode, locale)
        ) STRICT;
        CREATE TABLE label(
          locale TEXT NOT NULL REFERENCES declared_locale(locale),
          category TEXT NOT NULL CHECK(category IN ('single-card-section','three-card-position','relation')),
          label_id TEXT NOT NULL,
          value TEXT NOT NULL CHECK(length(trim(value)) > 0),
          PRIMARY KEY(locale, category, label_id)
        ) STRICT;
        CREATE TABLE vocabulary(
          locale TEXT NOT NULL REFERENCES declared_locale(locale),
          concept_id TEXT NOT NULL,
          label TEXT NOT NULL CHECK(length(trim(label)) > 0),
          meaning TEXT NOT NULL CHECK(length(trim(meaning)) > 0),
          PRIMARY KEY(locale, concept_id)
        ) STRICT;
        CREATE TABLE single_card(
          locale TEXT NOT NULL REFERENCES declared_locale(locale),
          card_id TEXT NOT NULL,
          orientation TEXT NOT NULL CHECK(orientation IN ('upright','reversed')),
          situation TEXT NOT NULL CHECK(length(trim(situation)) > 0),
          development TEXT NOT NULL CHECK(length(trim(development)) > 0),
          risk TEXT NOT NULL CHECK(length(trim(risk)) > 0),
          outcome TEXT NOT NULL CHECK(length(trim(outcome)) > 0),
          advice TEXT NOT NULL CHECK(length(trim(advice)) > 0),
          overall_valence INTEGER NOT NULL CHECK(overall_valence BETWEEN -2 AND 2),
          overall_intensity INTEGER NOT NULL CHECK(overall_intensity BETWEEN 1 AND 3),
          PRIMARY KEY(locale, card_id, orientation)
        ) STRICT;
        CREATE TABLE single_card_reversal_mechanism(
          locale TEXT NOT NULL,
          card_id TEXT NOT NULL,
          orientation TEXT NOT NULL,
          ordinal INTEGER NOT NULL CHECK(ordinal >= 0),
          mechanism TEXT NOT NULL CHECK(mechanism IN ('blocked','delayed','internalized','excessive','distorted','resisted','depleted')),
          PRIMARY KEY(locale, card_id, orientation, ordinal),
          UNIQUE(locale, card_id, orientation, mechanism),
          FOREIGN KEY(locale, card_id, orientation) REFERENCES single_card(locale, card_id, orientation)
        ) STRICT;
        CREATE TABLE single_card_tag(
          locale TEXT NOT NULL,
          card_id TEXT NOT NULL,
          orientation TEXT NOT NULL,
          ordinal INTEGER NOT NULL CHECK(ordinal >= 0),
          concept_id TEXT NOT NULL,
          valence INTEGER NOT NULL CHECK(valence BETWEEN -2 AND 2),
          intensity INTEGER NOT NULL CHECK(intensity BETWEEN 1 AND 3),
          PRIMARY KEY(locale, card_id, orientation, ordinal),
          UNIQUE(locale, card_id, orientation, concept_id),
          FOREIGN KEY(locale, card_id, orientation) REFERENCES single_card(locale, card_id, orientation),
          FOREIGN KEY(locale, concept_id) REFERENCES vocabulary(locale, concept_id)
        ) STRICT;
        CREATE TABLE oriented_pair(
          locale TEXT NOT NULL REFERENCES declared_locale(locale),
          card_a_id TEXT NOT NULL,
          card_b_id TEXT NOT NULL CHECK(card_a_id < card_b_id),
          orientation_state TEXT NOT NULL CHECK(orientation_state IN ('upright-upright','upright-reversed','reversed-upright','reversed-reversed')),
          interaction TEXT NOT NULL CHECK(length(trim(interaction)) > 0),
          direction TEXT NOT NULL CHECK(length(trim(direction)) > 0),
          overall_valence INTEGER NOT NULL CHECK(overall_valence BETWEEN -2 AND 2),
          overall_intensity INTEGER NOT NULL CHECK(overall_intensity BETWEEN 1 AND 3),
          PRIMARY KEY(locale, card_a_id, card_b_id, orientation_state)
        ) STRICT;
        CREATE TABLE oriented_pair_tag(
          locale TEXT NOT NULL,
          card_a_id TEXT NOT NULL,
          card_b_id TEXT NOT NULL,
          orientation_state TEXT NOT NULL,
          ordinal INTEGER NOT NULL CHECK(ordinal >= 0),
          concept_id TEXT NOT NULL,
          valence INTEGER NOT NULL CHECK(valence BETWEEN -2 AND 2),
          intensity INTEGER NOT NULL CHECK(intensity BETWEEN 1 AND 3),
          PRIMARY KEY(locale, card_a_id, card_b_id, orientation_state, ordinal),
          UNIQUE(locale, card_a_id, card_b_id, orientation_state, concept_id),
          FOREIGN KEY(locale, card_a_id, card_b_id, orientation_state) REFERENCES oriented_pair(locale, card_a_id, card_b_id, orientation_state),
          FOREIGN KEY(locale, concept_id) REFERENCES vocabulary(locale, concept_id)
        ) STRICT;
        CREATE TABLE three_card_position(
          locale TEXT NOT NULL REFERENCES declared_locale(locale),
          position TEXT NOT NULL CHECK(position IN ('past','present','future')),
          card_id TEXT NOT NULL,
          orientation TEXT NOT NULL CHECK(orientation IN ('upright','reversed')),
          text TEXT NOT NULL CHECK(length(trim(text)) > 0),
          overall_valence INTEGER NOT NULL CHECK(overall_valence BETWEEN -2 AND 2),
          overall_intensity INTEGER NOT NULL CHECK(overall_intensity BETWEEN 1 AND 3),
          PRIMARY KEY(locale, position, card_id, orientation)
        ) STRICT;
        CREATE TABLE three_card_position_tag(
          locale TEXT NOT NULL,
          position TEXT NOT NULL,
          card_id TEXT NOT NULL,
          orientation TEXT NOT NULL,
          ordinal INTEGER NOT NULL CHECK(ordinal >= 0),
          concept_id TEXT NOT NULL,
          valence INTEGER NOT NULL CHECK(valence BETWEEN -2 AND 2),
          intensity INTEGER NOT NULL CHECK(intensity BETWEEN 1 AND 3),
          PRIMARY KEY(locale, position, card_id, orientation, ordinal),
          UNIQUE(locale, position, card_id, orientation, concept_id),
          FOREIGN KEY(locale, position, card_id, orientation) REFERENCES three_card_position(locale, position, card_id, orientation),
          FOREIGN KEY(locale, concept_id) REFERENCES vocabulary(locale, concept_id)
        ) STRICT;
        CREATE TABLE synthesis_resource(
          locale TEXT NOT NULL REFERENCES declared_locale(locale),
          resource_type TEXT NOT NULL CHECK(resource_type IN ('three-card-position','trajectory-profile','synthesis-fragment','relation-label')),
          resource_id TEXT NOT NULL,
          canonical_json TEXT NOT NULL CHECK(json_valid(canonical_json)),
          PRIMARY KEY(locale, resource_type, resource_id)
        ) STRICT;
        """;
}

