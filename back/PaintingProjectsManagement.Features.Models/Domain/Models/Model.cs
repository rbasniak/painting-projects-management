using rbkApiModules.Commons.Core.Database;
using System.Text.Json.Serialization;

namespace PaintingProjectsManagement.Features.Models;

public class Model : TenantEntity
{
    // Constructor for EF Core, do not remove it or make it public
    [JsonConstructor]
    private Model() { }

    public Model(string tenant, string name, ModelCategory category, string[] characters, string franchise, ModelType type, string artist, string[] tags,
        BaseSize baseSize, FigureSize figureSize, int numberOfFigures, int sizeInMb)
    {
        Id = Guid.CreateVersion7();
        TenantId = tenant;

        Name = name;
        Category = category;
        CategoryId = category.Id;
        Artist = artist;
        Tags = tags;
        Characters = characters;
        Score = new Rating(0);
        BaseSize = baseSize;
        FigureSize = figureSize;
        NumberOfFigures = numberOfFigures;
        Type = type;
        Franchise = franchise;
        SizeInMb = sizeInMb;
        MustHave = false;

        RaiseDomainEvent(new ModelCreated(
            Id,
            Name,
            Category.Id,
            Artist,
            Franchise,
            Type,
            Tags,
            Characters,
            BaseSize,
            FigureSize,
            NumberOfFigures,
            SizeInMb,
            MustHave,
            Score.Value,
            PictureUrl
        ));
    }

    public string Name { get; private set; } = string.Empty;
    public string Franchise { get; private set; } = string.Empty;
    [JsonColumn]
    public string[] Characters { get; private set; } = [];
    public Guid CategoryId { get; private set; }
    public ModelCategory Category { get; private set; } = default!;
    public ModelType Type { get; private set; }
    public string Artist { get; private set; } = string.Empty;
    public string[] Tags { get; private set; } = Array.Empty<string>();
    public string? PictureUrl { get; private set; }
    public Rating Score { get; private set; } = default!;
    public BaseSize BaseSize { get; private set; }
    public FigureSize FigureSize { get; private set; }
    public int NumberOfFigures { get; private set; }
    public int SizeInMb { get; private set; } = 0;
    public bool MustHave { get; private set; }

    public void UpdateDetails(string name, ModelCategory category, string[] characters, string artist, string[] tags,
        BaseSize baseSize, FigureSize figureSize, int numberOfFigures, string franchise, ModelType type, int sizeInMb)
    {
        var changed = false;

        if (Name != name)
        {
            Name = name;
            changed = true;
        }

        if (Category != category)
        {
            Category = category;
            CategoryId = category.Id;
            changed = true;
        }

        if (Artist != artist)
        {
            Artist = artist;
            changed = true;
        }

        var newCharacters = characters ?? Array.Empty<string>();
        if (!Characters.SequenceEqual(newCharacters))
        {
            Characters = newCharacters;
            changed = true;
        }

        var newTags = tags ?? Array.Empty<string>();
        if (!Tags.SequenceEqual(newTags))
        {
            Tags = newTags;
            changed = true;
        }

        if (BaseSize != baseSize)
        {
            BaseSize = baseSize;
            changed = true;
        }

        if (FigureSize != figureSize)
        {
            FigureSize = figureSize;
            changed = true;
        }

        var newNumberOfFigures = numberOfFigures > 0 ? numberOfFigures : 1;
        if (NumberOfFigures != newNumberOfFigures)
        {
            NumberOfFigures = newNumberOfFigures;
            changed = true;
        }

        if (Franchise != franchise)
        {
            Franchise = franchise;
            changed = true;
        }

        if (Type != type)
        {
            Type = type;
            changed = true;
        }

        if (SizeInMb != sizeInMb)
        {
            SizeInMb = sizeInMb;
            changed = true;
        }

        if (changed)
        {
            RaiseDomainEvent(new ModelDetailsChanged(Id));
        }
    }

    public void UpdatePicture(string pictureUrl)
    {
        if (PictureUrl == pictureUrl)
        {
            return;
        }

        PictureUrl = pictureUrl;

        RaiseDomainEvent(new ModelPictureChanged(Id));
    }

    public void Rate(int score)
    {
        var newScore = new Rating(Math.Min(Math.Max(score, 0), 5));

        if (Score.Value == newScore.Value)
        {
            return;
        }

        Score = newScore;

        RaiseDomainEvent(new ModelRated(Id, Score.Value));
    }

    public void SetMustHave(bool mustHave)
    {
        if (MustHave == mustHave)
        {
            return;
        }

        MustHave = mustHave;

        RaiseDomainEvent(new ModelMustHaveChanged(Id, MustHave));
    }
}
