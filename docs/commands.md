# Migrations 
`dotnet ef migrations add <migration_name> --context AppDbContext --project Infrastructure --startup-project Api --output-dir Data/Migrations`

`dotnet ef database update --context AppDbContext --project Infrastructure --startup-project Api`

# Frontend tests 
`pnpm vitest`

# Fly deployment
For creating a new fly app: `fly launch --name api --no-deploy`, then edit .toml file manually to add in `Dockerfile`

For deploying a specific fly app: `fly deploy -c ../fly/api.toml` run this from the specific project directory (e.g. `./server` in this case) to prevent needlessly sending files to the builder.

# Fly env variables
.net variables get turned into the format: 
`ExternalApis__LanguageAnalysis__BaseUrl`

The address for an internal service is `http://<service name>.internal:<port>/`

E.g. the address of the python service is `http://language-python-silver-grove-876.internal:8000/`

If the secret is an array, append __<index> to the secret e.g. 

`Cors__AllowedOrigins__0`

