#!/bin/bash

# API Key Generator Script
# Usage: ./generate-api-keys.sh

echo "=========================================="
echo "  API Key Generator for Microservices"
echo "=========================================="
echo ""

# Function to generate a secure random API key
generate_api_key() {
    local service_name=$1
    # Generate 32 bytes random hex string
    local api_key=$(openssl rand -hex 32)
    echo "${service_name}-${api_key}"
}

echo "Generating API Keys for services..."
echo ""

# Generate keys for each service
READING_SERVICE_KEY=$(generate_api_key "reading")
COMIC_SERVICE_KEY=$(generate_api_key "comic")
USER_SERVICE_KEY=$(generate_api_key "user")

echo "✅ API Keys Generated:"
echo ""
echo "ReadingService API Key:"
echo "$READING_SERVICE_KEY"
echo ""
echo "ComicService API Key:"
echo "$COMIC_SERVICE_KEY"
echo ""
echo "UserService API Key:"
echo "$USER_SERVICE_KEY"
echo ""

# Create .env file for Docker Compose
cat > .env.apikeys << EOF
# Generated API Keys - DO NOT COMMIT TO GIT
# Generated at: $(date)

# ReadingService API Key
READING_SERVICE_API_KEY=$READING_SERVICE_KEY

# ComicService API Key
COMIC_SERVICE_API_KEY=$COMIC_SERVICE_KEY

# UserService API Key
USER_SERVICE_API_KEY=$USER_SERVICE_KEY
EOF

echo "=========================================="
echo "✅ API Keys saved to: .env.apikeys"
echo ""
echo "⚠️  IMPORTANT SECURITY NOTES:"
echo "1. Add .env.apikeys to .gitignore"
echo "2. Share keys securely (use password manager)"
echo "3. Rotate keys every 30 days in production"
echo "4. Never commit keys to version control"
echo "=========================================="
echo ""
echo "📋 Next Steps:"
echo "1. Copy the keys above to your appsettings.json files"
echo "2. Or use environment variables from .env.apikeys"
echo "3. Restart all services"
echo "=========================================="
