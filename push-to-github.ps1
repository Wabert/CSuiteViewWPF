# GitHub Repository Setup Script
# Run this after creating your repository on GitHub

# Replace YOUR_USERNAME with your actual GitHub username
$GITHUB_USERNAME = "YOUR_USERNAME"
$REPO_NAME = "CSuiteViewWPF"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CSuiteViewWPF - GitHub Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to project directory
cd "C:\Users\ab7y02\OneDrive - American National Insurance Company\Documents\Code Projects\C#\CSuiteView\CSuiteViewWPF"

# Instructions
Write-Host "STEP 1: Go to https://github.com/new" -ForegroundColor Yellow
Write-Host "  - Repository name: CSuiteViewWPF" -ForegroundColor White
Write-Host "  - Description: Modern WPF application with layered UI design" -ForegroundColor White
Write-Host "  - Choose Public or Private" -ForegroundColor White
Write-Host "  - DO NOT initialize with README (we have one)" -ForegroundColor White
Write-Host ""

Write-Host "STEP 2: After creating the repo, update the username in this script" -ForegroundColor Yellow
Write-Host "  Edit this file and replace YOUR_USERNAME with your GitHub username" -ForegroundColor White
Write-Host ""

if ($GITHUB_USERNAME -eq "YOUR_USERNAME") {
    Write-Host "⚠️  Please edit this script and set your GitHub username first!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Press Enter to exit..."
    Read-Host
    exit
}

Write-Host "STEP 3: Running git commands..." -ForegroundColor Yellow
Write-Host ""

# Add remote
Write-Host "Adding remote origin..." -ForegroundColor Green
git remote add origin "https://github.com/$GITHUB_USERNAME/$REPO_NAME.git"

# Rename branch to main
Write-Host "Renaming branch to main..." -ForegroundColor Green
git branch -M main

# Push to GitHub
Write-Host "Pushing to GitHub..." -ForegroundColor Green
git push -u origin main

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ Done! Your repository is now on GitHub!" -ForegroundColor Green
Write-Host "Visit: https://github.com/$GITHUB_USERNAME/$REPO_NAME" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
