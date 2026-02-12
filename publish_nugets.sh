dotnet pack --configuration Release --include-symbols --output ./nugets

for nupkg in ./nugets/*.nupkg; do
  dotnet nuget push "$nupkg" --source https://api.nuget.org/v3/index.json --skip-duplicate --api-key "$API_KEY"
done