namespace Praedora.Core.Entities;

public record JobDescriptionExtract(
    string[] RequiredSkills,
    string[] NiceToHave,
    string? SalaryRangeText,
    string? Location,
    string? RemotePolicy,
    int? YearsExperience);
