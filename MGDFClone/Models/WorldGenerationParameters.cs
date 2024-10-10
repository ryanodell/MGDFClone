using MGDFClone.Features.WorldGen;

namespace MGDFClone.Models;
public class WorldGenerationParameters {
    public eWorldSize WorldSize { get; set; } = eWorldSize.Small;
    public required ElevationParameters ElevationParameters { get; set; }
    public required WorldTemperatureParameters WorldTemperatureParameters { get; set; }

}
