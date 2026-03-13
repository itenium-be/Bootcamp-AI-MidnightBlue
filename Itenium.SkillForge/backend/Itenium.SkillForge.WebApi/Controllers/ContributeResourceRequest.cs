using System.ComponentModel.DataAnnotations;
using Itenium.SkillForge.Entities;

namespace Itenium.SkillForge.WebApi.Controllers;

public record ContributeResourceRequest(
    [Required][MaxLength(200)] string Title,
    [Required][MaxLength(500)][Url] string Url,
    [Required] ResourceType Type,
    [Required] int SkillId,
    int? FromLevel,
    int? ToLevel,
    [MaxLength(2000)] string? Description);
