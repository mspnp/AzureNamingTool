# Git Workflow Guide for AzureNamingTool Development

## Branch Structure

### Your Fork (BryanSoltis/AzureNamingTool-DEV)
- `main` - Clean history (no large files), synced with your work
- `dev` - Development branch, synced with upstream mspnp/dev
- `feature/*` - Feature branches for new work

### Upstream (mspnp/AzureNamingTool)
- `main` - Production releases
- `dev` - Development/staging branch

## Workflow

### 1. Starting New Work

```powershell
# Ensure you're on dev and it's up to date with upstream
git checkout dev
git fetch upstream
git merge upstream/dev
git push origin dev

# Create a feature branch
git checkout -b feature/your-feature-name
```

### 2. During Development

```powershell
# Make your changes and commit
git add .
git commit -m "Your commit message"

# Push to your fork
git push origin feature/your-feature-name
```

### 3. PR to Your Fork's Dev

```powershell
# Create PR: feature/your-feature-name → BryanSoltis/dev
# Review and merge via GitHub UI
```

### 4. PR to Upstream Dev

```powershell
# After merging to your dev, create PR:
# BryanSoltis/dev → mspnp/dev
# This will be reviewed by upstream maintainers
```

### 5. Upstream Releases Main

```powershell
# Upstream team creates PR: mspnp/dev → mspnp/main
# After release, sync your fork
git checkout main
git fetch upstream
git merge upstream/main
git push origin main
```

## Fixing Current Situation

### Problem
The history rewrite (removing large zip files) made your branches incompatible with upstream.

### Solution

#### Option 1: Reset to Upstream History (Recommended)
```powershell
# Backup your current work
git branch backup-main main
git branch backup-dev dev

# Reset to upstream (keeps large files in history)
git fetch https://github.com/mspnp/AzureNamingTool.git
git checkout main
git reset --hard FETCH_HEAD
git push origin main --force-with-lease

git checkout dev  
git fetch https://github.com/mspnp/AzureNamingTool.git dev
git reset --hard FETCH_HEAD
git push origin dev --force-with-lease

# Create new feature branch with your v5.0.0 work
git checkout -b feature/v5.0.0-updates dev
# Manually apply your changes or cherry-pick from backup branches
```

#### Option 2: Continue with Clean History (Your Fork Only)
Keep your current clean history but create a separate branch for upstream PRs:

```powershell
# Add upstream remote
git remote add upstream https://github.com/mspnp/AzureNamingTool.git

# Create upstream-compatible branch
git fetch upstream dev
git checkout -b upstream-dev upstream/dev

# Manually apply your v5.0.0 changes to this branch
# (Copy files, commit, push)
git push origin upstream-dev

# PR: BryanSoltis/upstream-dev → mspnp/dev
```

## Daily Workflow (After Fix)

### Working on a new feature
```powershell
git checkout dev
git pull origin dev
git checkout -b feature/new-awesome-feature
# ... make changes ...
git commit -am "Add awesome feature"
git push origin feature/new-awesome-feature
```

### Create PR to your dev via GitHub UI
- Go to GitHub
- Create PR: `feature/new-awesome-feature` → `BryanSoltis/dev`
- Review and merge

### Create PR to upstream
- Ensure your `dev` is merged
- Go to upstream repo: https://github.com/mspnp/AzureNamingTool
- Click "New Pull Request"
- Set base: `mspnp/dev`, compare: `BryanSoltis/dev`
- Fill in description and submit

## CI/CD Recommendations

### Add .gitignore for large files
```gitignore
# Deployment artifacts
*.zip
publish/
publish-output/
TestResults/
app-logs/
deploy/

# Keep small test files
!tests/**/*.zip
```

### Add GitHub Actions for automated deployment
Create `.github/workflows/build-and-deploy.yml`:

```yaml
name: Build and Deploy

on:
  push:
    branches: [ dev ]
  pull_request:
    branches: [ dev ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
    - name: Restore dependencies
      run: dotnet restore src/AzureNamingTool.csproj
    - name: Build
      run: dotnet build src/AzureNamingTool.csproj --no-restore -c Release
    - name: Test
      run: dotnet test tests/AzureNamingTool.UnitTests/AzureNamingTool.UnitTests.csproj --no-build -c Release
```

## Deployment Strategy

### For Azure App Service
Use GitHub Actions to build and deploy directly to Azure:

```yaml
- name: Publish
  run: dotnet publish src/AzureNamingTool.csproj -c Release -o ./publish
  
- name: Deploy to Azure Web App
  uses: azure/webapps-deploy@v2
  with:
    app-name: ${{ secrets.AZURE_WEBAPP_NAME }}
    publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
    package: ./publish
```

### For Docker
Build and push to container registry:

```yaml
- name: Build Docker image
  run: docker build -t your-registry/azurenamingtool:${{ github.sha }} .
  
- name: Push to registry
  run: docker push your-registry/azurenamingtool:${{ github.sha }}
```

## Best Practices

1. **Never commit large files** - Add to .gitignore first
2. **Small, focused commits** - Easier to review and revert
3. **Descriptive commit messages** - Explain why, not just what
4. **Test before PR** - Run build and tests locally
5. **Keep dev in sync** - Regularly pull from upstream
6. **Use feature branches** - Never commit directly to dev or main
7. **Review your own PR first** - Check the diff before requesting review

## Useful Git Commands

```powershell
# See what changed
git status
git diff

# Undo unstaged changes
git restore <file>

# Undo last commit (keep changes)
git reset --soft HEAD~1

# Update from upstream
git fetch upstream
git merge upstream/dev

# Clean up old branches
git branch -d feature/old-branch
git push origin --delete feature/old-branch

# See branch history
git log --oneline --graph --all

# Find large files in history
git rev-list --objects --all | git cat-file --batch-check='%(objecttype) %(objectname) %(objectsize) %(rest)' | awk '/^blob/ {print $2, $3, $4}' | sort -k2 -n
```
