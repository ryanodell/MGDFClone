using MGDFClone.Models;

namespace MGDFClone.Features.WorldGen;
public class CivilizationManagerV1 {
    private RegionTile1[] m_RegionTiles;
    private IDictionary<eRace, RacePreference> m_RacePreferences = new Dictionary<eRace, RacePreference> {
        { eRace.Dwarf, new RacePreference {
            MinTemperature = -10,
            MaxTemperature = 10,
            MinHumidity = 20,
            MaxHumidity = 50,
            PreferredMaxElevation = 0.85f,
            PreferredMinElevation = 0.90f
            }
        },
        { eRace.Human, new RacePreference {
            MinTemperature = 10,
            MaxTemperature = 30,
            MinHumidity = 30,
            MaxHumidity = 60,
            PreferredMaxElevation = 0.40f,
            PreferredMinElevation = 0.70f
            }
        }
    };
    public CivilizationManagerV1(RegionTile1[] regionTiles) {
        m_RegionTiles = regionTiles;
    }

    private float _evaluateLocationForRace(RegionTile1 regionTile, RacePreference racePreference) {
        float score = 0.0f;
        if (regionTile.Temperature >= racePreference.MinTemperature && regionTile.Temperature <= racePreference.MaxTemperature) {
            score += 10.0f;
        }
        if (regionTile.Humidity >= racePreference.MinHumidity && regionTile.Humidity <= racePreference.MaxHumidity) {
            score += 10.0f;
        }
        return score;
    }

    private static Random _random = new Random();

    public void GenerateCivilizations() {
        var dwarfPreference = m_RacePreferences[eRace.Dwarf];
        var humanPreference = m_RacePreferences[eRace.Human];
        for (int i = 0; i < m_RegionTiles.Length; i++) {
            var regionTile = m_RegionTiles[i];
            float dwarfScore = _evaluateLocationForRace(regionTile, dwarfPreference);
            float humanScore = _evaluateLocationForRace(regionTile, humanPreference);
            if (dwarfScore > 90) {
                var chance = _random.Next(100);
                if (chance >= 90) {
                    regionTile.Civilization = eRace.Dwarf;
                }
            }
            if (humanScore > 90) {
                var chance = _random.Next(100);
                if (chance >= 75) {
                    regionTile.Civilization = eRace.Human;
                }
            }
        }
    }
}

public class RacePreference {
    public float MinTemperature { get; set; }
    public float MaxTemperature { get; set; }
    public float MinHumidity { get; set; }
    public float MaxHumidity { get; set; }
    public float PreferredMinElevation { get; set; }
    public float PreferredMaxElevation { get; set; }
}



