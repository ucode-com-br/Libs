# Get the directory where the PS1 script is located
$projectRoot = "./"

# Function to remove the content of a directory and ignore errors
function Remove-DirectoryContent {
    param (
        [string]$directory
    )
    try {
        # Check if the directory exists
        if (Test-Path $directory) {
            Write-Host "Removing contents of directory: $directory"
            # Get all files and folders inside the directory
            Remove-Item -Path $directory -Recurse -Force -ErrorAction SilentlyContinue
        } else {
            Write-Host "Directory does not exist: $directory"
        }
    } catch {
        Write-Host "Error removing contents of directory: $directory"
        # Ignore any errors
    }
}

# Recursively search for all "bin" and "obj" directories
$binDirs = Get-ChildItem -Path $projectRoot -Recurse -Directory -Filter "bin"
$objDirs = Get-ChildItem -Path $projectRoot -Recurse -Directory -Filter "obj"

# Remove the content of all "bin" directories
foreach ($binDir in $binDirs) {
    Remove-DirectoryContent -directory $binDir.FullName
}

# Remove the content of all "obj" directories
foreach ($objDir in $objDirs) {
    Remove-DirectoryContent -directory $objDir.FullName
}

Write-Host "Cleanup of 'bin' and 'obj' folders completed."