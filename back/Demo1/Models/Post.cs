﻿using rbkApiModules.Commons.Core.UiDefinitions;

namespace Demo1.Models;

public class Post : TenantEntity
{
    private Post()
    {
        Blog = default!;   
        Author = default!; 
    }

    public Post(string tenant, Blog blog, Author author, string title, string body, string uniqueInTenant, string uniqueInApplication, DateTime? publishDate = null)
    {
        UniqueInTenant = uniqueInTenant;
        UniqueInApplication = uniqueInApplication;
        TenantId = tenant;
        Title = title;
        Body = body;
        Author = author;
        Blog = blog;
        PublishingDate = publishDate;
    }

    [Required]
    [MinLength(2), MaxLength(16)]
    [DialogData(OperationType.CreateAndUpdate, "Título")]
    public string Title { get; private set; } = string.Empty;

    [MinLength(32), MaxLength(4096)]
    [DialogData(OperationType.CreateAndUpdate, "Texto")]
    public string Body { get; private set; } = string.Empty;

    [DialogData(OperationType.Create, "Data de Publicação")]
    public DateTime? PublishingDate { get; private set; }

    [DialogData(OperationType.CreateAndUpdate, "Autor")]
    public Guid AuthorId { get; private set; }
    public Author Author { get; private set; }

    [DialogData(OperationType.CreateAndUpdate, "Blogui")]
    public Guid BlogId { get; private set; } 
    public Blog Blog { get; private set; }

    [Required]
    [MinLength(1), MaxLength(32)]
    [DialogData(OperationType.CreateAndUpdate, "Valor único no sistema")]
    public string UniqueInApplication { get; private set; } = string.Empty;

    [Required]
    [MinLength(1), MaxLength(32)]
    [DialogData(OperationType.CreateAndUpdate, "Valor único no tenant")]
    public string UniqueInTenant { get; private set; } = string.Empty;

    public void Update(string title, string body, string uniqueInApplication, string uniqueInTenant)
    {
        UniqueInApplication = uniqueInApplication;
        UniqueInTenant = uniqueInTenant;
        Title = title;
        Body = body;
    }
}