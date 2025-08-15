#!/usr/bin/env python3
"""
Script to convert the Markdown game architecture documentation to Word format.
This script provides instructions and can be used with pandoc or similar tools.
"""

import os
import subprocess
import sys

def check_pandoc():
    """Check if pandoc is installed."""
    try:
        subprocess.run(['pandoc', '--version'], capture_output=True, check=True)
        return True
    except (subprocess.CalledProcessError, FileNotFoundError):
        return False

def convert_with_pandoc():
    """Convert Markdown to Word using pandoc."""
    input_file = "Game_Architecture_Documentation.md"
    output_file = "Game_Architecture_Documentation.docx"
    
    if not os.path.exists(input_file):
        print(f"Error: {input_file} not found!")
        return False
    
    try:
        # Convert markdown to Word document
        cmd = [
            'pandoc',
            input_file,
            '-o', output_file,
            '--from', 'markdown',
            '--to', 'docx',
            '--standalone',
            '--toc',  # Add table of contents
            '--number-sections'  # Number sections
        ]
        
        result = subprocess.run(cmd, capture_output=True, text=True)
        
        if result.returncode == 0:
            print(f"✅ Successfully converted to {output_file}")
            print(f"📄 File location: {os.path.abspath(output_file)}")
            return True
        else:
            print(f"❌ Conversion failed: {result.stderr}")
            return False
            
    except Exception as e:
        print(f"❌ Error during conversion: {e}")
        return False

def manual_conversion_instructions():
    """Provide manual conversion instructions."""
    print("\n" + "="*60)
    print("MANUAL CONVERSION INSTRUCTIONS")
    print("="*60)
    print("\nIf pandoc is not available, you can manually convert the documentation:")
    print("\n1. 📋 Copy the content from 'Game_Architecture_Documentation.md'")
    print("2. 📝 Open Microsoft Word")
    print("3. 📄 Create a new document")
    print("4. 📋 Paste the content")
    print("5. 🎨 Apply formatting:")
    print("   - Use Heading 1 for main sections")
    print("   - Use Heading 2 for subsections")
    print("   - Use Heading 3 for sub-subsections")
    print("   - Format code blocks with monospace font")
    print("   - Add page breaks between major sections")
    print("6. 📊 For the architecture diagram:")
    print("   - Copy the ASCII diagram")
    print("   - Use a monospace font (like Courier New)")
    print("   - Or recreate it using Word's drawing tools")
    print("7. 💾 Save as .docx format")
    print("\n" + "="*60)

def main():
    """Main function."""
    print("🎮 Game Architecture Documentation Converter")
    print("="*50)
    
    # Check if pandoc is available
    if check_pandoc():
        print("✅ Pandoc found! Attempting automatic conversion...")
        if convert_with_pandoc():
            print("\n🎉 Conversion completed successfully!")
            print("📖 You can now open the Word document and format it as needed.")
        else:
            print("\n⚠️  Automatic conversion failed. Using manual instructions...")
            manual_conversion_instructions()
    else:
        print("❌ Pandoc not found. Using manual conversion instructions...")
        manual_conversion_instructions()
    
    print("\n📚 Additional Tips:")
    print("- The Markdown file contains all the content you need")
    print("- The ASCII diagram can be enhanced with Word's drawing tools")
    print("- Consider adding screenshots of the game for visual appeal")
    print("- Use Word's built-in table of contents feature")

if __name__ == "__main__":
    main()
