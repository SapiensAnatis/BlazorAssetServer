using System.ComponentModel.DataAnnotations;
using AssetServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AssetServer;

[Route("dl/manifests")]
[ApiController]
public class ManifestsController : ControllerBase
{
    private const int MatchTimeout = 1000;
    private const string PlatformRegex = "iOS|Android";
    private const string ManifestNameRegex = @"[a-zA-Z0-9]{16}";
    private const string ManifestFileRegex = @"assetbundle\.(en_eu|en_us|zh_cn|zh_tw)?\.?manifest";

    private readonly ConfigurationService configurationService;
    private readonly FileRetrievalService fileRetrievalService;

    public ManifestsController(
        ConfigurationService configurationService,
        FileRetrievalService fileRetrievalService
    )
    {
        this.configurationService = configurationService;
        this.fileRetrievalService = fileRetrievalService;
    }

    [HttpGet("{platform}/{manifestName}/{manifestFile}")]
    public IActionResult GetManifest(
        [FromRoute]
        [RegularExpression(PlatformRegex, MatchTimeoutInMilliseconds = MatchTimeout)]
            string platform,
        [FromRoute]
        [RegularExpression(ManifestNameRegex, MatchTimeoutInMilliseconds = MatchTimeout)]
            string manifestName,
        [FromRoute]
        [RegularExpression(ManifestFileRegex, MatchTimeoutInMilliseconds = MatchTimeout)]
            string manifestFile
    )
    {
        string path = Path.Combine(
            configurationService.CurrentValue.ManifestPath,
            manifestName,
            manifestFile
        );

        if (!this.fileRetrievalService.TryLoadFile(path, out FileStream? stream))
            return NotFound();

        return File(stream, "application/octet-stream");
    }
}
