#!/bin/bash
#
# Build and Package InfraSizing Calculator for Windows Server Deployment
#
# This script:
# - Builds the application in Release mode
# - Publishes for Windows deployment
# - Creates a ZIP package for transfer
#
# Usage:
#   ./build-and-package.sh                    # Uses defaults
#   ./build-and-package.sh -o ~/Desktop       # Custom output directory
#   ./build-and-package.sh -c Development     # Custom configuration
#

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Default values
OUTPUT_DIR="./publish"
CONFIGURATION="Release"
PROJECT_PATH="src/InfraSizingCalculator/InfraSizingCalculator.csproj"
PACKAGE_NAME="InfraSizing"

# Parse command line arguments
while getopts "o:c:p:n:h" opt; do
    case $opt in
        o) OUTPUT_DIR="$OPTARG" ;;
        c) CONFIGURATION="$OPTARG" ;;
        p) PROJECT_PATH="$OPTARG" ;;
        n) PACKAGE_NAME="$OPTARG" ;;
        h)
            echo "Usage: $0 [-o output_dir] [-c configuration] [-p project_path] [-n package_name]"
            echo ""
            echo "Options:"
            echo "  -o    Output directory (default: ./publish)"
            echo "  -c    Build configuration (default: Release)"
            echo "  -p    Project file path (default: src/InfraSizingCalculator/InfraSizingCalculator.csproj)"
            echo "  -n    Package name (default: InfraSizing)"
            echo "  -h    Show this help message"
            exit 0
            ;;
        \?)
            echo "Invalid option: -$OPTARG" >&2
            exit 1
            ;;
    esac
done

# Find script directory and project root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

echo -e "${CYAN}========================================"
echo "InfraSizing Calculator - Build & Package"
echo -e "========================================${NC}"
echo ""

# Change to project root
cd "$PROJECT_ROOT"
echo -e "Project root: ${CYAN}$PROJECT_ROOT${NC}"
echo -e "Configuration: ${CYAN}$CONFIGURATION${NC}"
echo ""

# Verify project exists
if [ ! -f "$PROJECT_PATH" ]; then
    echo -e "${RED}ERROR: Project file not found: $PROJECT_PATH${NC}"
    exit 1
fi

# Step 1: Clean previous builds
echo -e "${YELLOW}[1/5] Cleaning previous builds...${NC}"
rm -rf "$OUTPUT_DIR"
dotnet clean "$PROJECT_PATH" -c "$CONFIGURATION" --verbosity minimal
echo -e "${GREEN}  Clean complete${NC}"

# Step 2: Restore packages
echo ""
echo -e "${YELLOW}[2/5] Restoring packages...${NC}"
dotnet restore "$PROJECT_PATH" --verbosity minimal
echo -e "${GREEN}  Restore complete${NC}"

# Step 3: Build
echo ""
echo -e "${YELLOW}[3/5] Building application...${NC}"
dotnet build "$PROJECT_PATH" -c "$CONFIGURATION" --no-restore --verbosity minimal
echo -e "${GREEN}  Build complete${NC}"

# Step 4: Publish for Windows
echo ""
echo -e "${YELLOW}[4/5] Publishing for Windows Server...${NC}"
PUBLISH_DIR="$OUTPUT_DIR/app"
dotnet publish "$PROJECT_PATH" \
    -c "$CONFIGURATION" \
    -o "$PUBLISH_DIR" \
    --no-build \
    --verbosity minimal

# Count published files
FILE_COUNT=$(find "$PUBLISH_DIR" -type f | wc -l | tr -d ' ')
echo -e "${GREEN}  Published $FILE_COUNT files to $PUBLISH_DIR${NC}"

# Step 5: Create ZIP package
echo ""
echo -e "${YELLOW}[5/5] Creating deployment package...${NC}"
TIMESTAMP=$(date +%Y%m%d-%H%M%S)
ZIP_NAME="${PACKAGE_NAME}-${TIMESTAMP}.zip"
ZIP_PATH="$OUTPUT_DIR/$ZIP_NAME"

cd "$PUBLISH_DIR"
zip -r "../$ZIP_NAME" . -x "*.pdb" > /dev/null
cd "$PROJECT_ROOT"

ZIP_SIZE=$(ls -lh "$ZIP_PATH" | awk '{print $5}')
echo -e "${GREEN}  Package created: $ZIP_PATH ($ZIP_SIZE)${NC}"

# Also create a latest link
LATEST_ZIP="$OUTPUT_DIR/${PACKAGE_NAME}-latest.zip"
cp "$ZIP_PATH" "$LATEST_ZIP"

# Summary
echo ""
echo -e "${CYAN}========================================"
echo "Build Complete"
echo -e "========================================${NC}"
echo ""
echo "Output files:"
echo -e "  ${GREEN}Timestamped:${NC} $ZIP_PATH"
echo -e "  ${GREEN}Latest:${NC}      $LATEST_ZIP"
echo -e "  ${GREEN}App folder:${NC}  $PUBLISH_DIR"
echo ""
echo "Deployment options:"
echo ""
echo -e "${YELLOW}Option 1: SMB File Share${NC}"
echo "  1. Connect to server share: smb://server-ip/c$/inetpub"
echo "  2. Copy $ZIP_NAME to the server"
echo "  3. On server, run: .\\03-deploy.ps1 -SourcePath 'C:\\path\\to\\$ZIP_NAME'"
echo ""
echo -e "${YELLOW}Option 2: Direct SCP (if SSH enabled)${NC}"
echo "  scp $ZIP_PATH user@server-ip:C:/deploy/"
echo "  Then on server: .\\03-deploy.ps1 -SourcePath 'C:\\deploy\\$ZIP_NAME'"
echo ""
echo -e "${YELLOW}Option 3: Manual Copy${NC}"
echo "  1. Copy contents of $PUBLISH_DIR to server"
echo "  2. Or extract $ZIP_NAME directly on server"
echo ""
