#!/bin/bash
# Generate AES-256 Encryption Key
# This script generates a cryptographically secure 256-bit (32-byte) key encoded in Base64
# The key can be used for the EncryptionKey setting in appsettings.json

echo -e "\033[36mGenerating AES-256 Encryption Key...\033[0m"
echo ""

# Generate 32 random bytes and encode as Base64
BASE64_KEY=$(openssl rand -base64 32)

echo -e "\033[32mYour AES-256 Encryption Key (Base64):\033[0m"
echo -e "\033[33m$BASE64_KEY\033[0m"
echo ""

echo -e "\033[36mAdd this key to your appsettings.json:\033[0m"
echo -e "\033[90m{"
echo -e "  \"EncryptionKey\": \"\033[33m$BASE64_KEY\033[90m\""
echo -e "}\033[0m"
echo ""

echo -e "\033[36mOr set as environment variable:\033[0m"
echo -e "\033[90mexport EncryptionKey=\"$BASE64_KEY\"\033[0m"
echo ""

echo -e "\033[31mIMPORTANT: Keep this key secure!\033[0m"
echo -e "\033[31m- Do NOT commit it to source control\033[0m"
echo -e "\033[31m- Store it securely (Azure Key Vault, AWS Secrets Manager, etc.)\033[0m"
echo -e "\033[31m- Use different keys for development, staging, and production\033[0m"
echo ""

# Try to copy to clipboard if xclip is available (Linux)
if command -v xclip &> /dev/null; then
    echo "$BASE64_KEY" | xclip -selection clipboard
    echo -e "\033[32mKey copied to clipboard!\033[0m"
elif command -v pbcopy &> /dev/null; then
    # macOS
    echo "$BASE64_KEY" | pbcopy
    echo -e "\033[32mKey copied to clipboard!\033[0m"
else
    echo -e "\033[33mClipboard tool not found. Please copy the key manually.\033[0m"
fi
