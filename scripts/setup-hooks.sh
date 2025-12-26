#!/bin/bash
#
# Setup script for development environment
# Run this once after cloning the repository
#

echo "🔧 Setting up Infrastructure Sizing Calculator development environment..."
echo ""

# 1. Configure git hooks
echo "📌 Configuring git hooks..."
git config core.hooksPath .githooks
chmod +x .githooks/*
echo "   ✅ Git hooks configured"

# 2. Configure commit template
echo "📝 Configuring commit message template..."
git config commit.template .gitmessage
echo "   ✅ Commit template configured"

# 3. Display reminder
echo ""
echo "═══════════════════════════════════════════════════════════════"
echo "✅ SETUP COMPLETE"
echo "═══════════════════════════════════════════════════════════════"
echo ""
echo "Your environment is now configured to enforce the team workflow."
echo ""
echo "📋 What's configured:"
echo "   • Git hooks: Commits without phase tags will be blocked"
echo "   • Commit template: Use 'git commit' to see the template"
echo ""
echo "📖 Read these documents:"
echo "   • CONTRIBUTING.md - Contribution guidelines"
echo "   • docs/process/TEAM_WORKFLOW.md - Team workflow"
echo "   • docs/process/CHANGE_IMPACT_MATRIX.md - What docs to update"
echo ""
echo "🏷️  Remember: All commits need phase tags:"
echo "   [SPEC] - Specifications"
echo "   [IMPL] - Implementation"
echo "   [SYNC] - Documentation sync"
echo "   [TEST] - Tests"
echo ""
echo "═══════════════════════════════════════════════════════════════"
