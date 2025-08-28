# Models Seeding

This directory contains the seeding logic for models and model categories.

## How it works

The seeding process automatically detects whether it's running on the specific computer with the local 3D models library or not:

### On the specific computer (with local library)
- **Path checked**: `D:\Printing and Painting\3D Models\Figures`
- **Action**: Seeds models from the local disk library using `ModelsSeeder.SeedFromDisk()`
- **Additional action**: Generates a SQL seed file (`models_seed.sql`) with all categories and models saved in the database

### On other computers (without local library)
- **Action**: Seeds models from the SQL file (`models_seed.sql`) if it exists
- **If SQL file doesn't exist**: No seeding occurs

## Generated SQL File

The `models_seed.sql` file contains:
- Model categories with their IDs, tenant IDs, and names
- Models with all their properties including:
  - Basic info (name, franchise, artist, etc.)
  - JSON arrays for characters, tags, and pictures
  - Enums converted to integers (ModelType, BaseSize, FigureSize)
  - Rating score
  - File size and other metadata

## File Location

The SQL seed file is generated in: `{AppDomain.CurrentDomain.BaseDirectory}/Seed/models_seed.sql`

## Usage

The seeding is automatically triggered during database initialization. No manual intervention is required.

## Database Tables

The seeding works with these PostgreSQL tables:
- `public."models.categories"` - Model categories
- `public."models.models"` - Models with all their properties

## PostgreSQL Compatibility

The generated SQL uses PostgreSQL syntax:
- Double-quoted table and column names
- `true`/`false` for boolean values instead of 1/0
- No `SET IDENTITY_INSERT` (PostgreSQL handles this automatically)
- Proper escaping of single quotes in string values
- Dollar-quoted strings (`$$text$$`) for file paths to avoid backslash escaping issues
