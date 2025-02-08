import re
import argparse
import sys
import os
import subprocess

def convert_markdown_to_bbcode(markdown_text, repo_name=None, bbcode_type='egosoft', relative_path=None):
    """
    Converts Markdown formatted text to BBCode formatted text.

    Args:
        markdown_text (str): The Markdown text to be converted.
        repo_name (str, optional): GitHub repository name in the format 'username/repo'.
                                   Used to generate absolute URLs for images.
        bbcode_type (str): Type of BBCode format ('egosoft' or 'nexus').
        relative_path (str, optional): Relative path of the input file.

    Returns:
        str: The converted BBCode text.
    """

    bbcode_text = markdown_text

    # 1. Headers
    # Define size mapping based on BBCode type
    header_level_mapping = {
        'egosoft': {
            1: {'size': 140, 'underline': True, 'italic': False, 'bold': False},
            2: {'size': 130, 'underline': True, 'italic': False, 'bold': False},
            3: {'size': 125, 'underline': True, 'italic': False, 'bold': False},
            4: {'size': 120, 'underline': True, 'italic': False, 'bold': False},
            5: {'size': 115, 'underline': True, 'italic': False, 'bold': False},
            6: {'size': 110, 'underline': True, 'italic': False, 'bold': False}
        },
        'nexus': {
            1: {'size': 4, 'underline': True, 'italic': False, 'bold': False},
            2: {'size': 3, 'underline': True, 'italic': False, 'bold': True},
            3: {'size': 3, 'underline': True, 'italic': True, 'bold': False},
            4: {'size': 3, 'underline': True, 'italic': False, 'bold': False},
            5: {'size': 2, 'underline': True, 'italic': True, 'bold': False},
            6: {'size': 2, 'underline': True, 'italic': False, 'bold': False}
        }
    }

    current_header_level_mapping = header_level_mapping.get(bbcode_type, header_level_mapping['egosoft'])

    # Convert Markdown headers to BBCode with size, bold, underline, and italic
    def replace_headers(match):
        hashes, header_text = match.groups()
        level = len(hashes)
        header_attrs = current_header_level_mapping.get(level, {'size': 100, 'underline': True, 'italic': False, 'bold': True})
        size = header_attrs['size']
        underline = '[u]' if header_attrs['underline'] else ''
        italic = '[i]' if header_attrs['italic'] else ''
        bold = '[b]' if header_attrs['bold'] else ''
        underline_close = '[/u]' if header_attrs['underline'] else ''
        italic_close = '[/i]' if header_attrs['italic'] else ''
        bold_close = '[/b]' if header_attrs['bold'] else ''
        return f"[size={size}]{underline}{italic}{bold}{header_text.strip()}{bold_close}{italic_close}{underline_close}[/size]"

    bbcode_text = re.sub(r'^(#{1,6})\s+(.*)', replace_headers, bbcode_text, flags=re.MULTILINE)

    # 2. Bold
    # Convert **text** or __text__ to [b]text[/b]
    bbcode_text = re.sub(r'(\*\*|__)(.*?)\1', r'[b]\2[/b]', bbcode_text)

    # 3. Images
    def replace_images(match):
        image_url = match.group(1)
        if repo_name and not re.match(r'^https?://', image_url):
            absolute_url = f"https://github.com/{repo_name}/raw/main/{relative_path}/{image_url}"
            if bbcode_type == 'egosoft':
                return f"[spoiler][img]{absolute_url}[/img][/spoiler]"
            else:
                return f"[img]{absolute_url}[/img]"
        else:
            if bbcode_type == 'egosoft':
                return f"[spoiler][img]{image_url}[/img][/spoiler]"
            else:
                return f"[img]{image_url}[/img]"

    bbcode_text = re.sub(r'!\[.*?\]\((.*?)\)', replace_images, bbcode_text)

    # 4. Italics
    # Convert *text* or _text_ to [i]text[/i]
    # Only match if the marker is preceded by a space or start of line
    # This prevents matching underscores within URLs or words like some_word
    bbcode_text = re.sub(r'(^|\s)(\*|_)(?!\2)(.*?)\2', r'\1[i]\3[/i]', bbcode_text)

    # 5. Inline Code
    # Convert `text` to [b]text[/b]
    bbcode_text = re.sub(r'`([^`\n]+)`', r'[b]\1[/b]', bbcode_text)

    # 6. Block Code
    # Convert ```language\ncode\n``` to [code=language]code[/code] for 'egosoft'
    # and to [code]code[/code] for 'nexus'
    def replace_block_code(match):
        lang = match.group(1) if match.group(1) else ''
        code = match.group(2)
        if bbcode_type != 'nexus' and lang:
            return f"[code={lang}]\n{code}\n[/code]"
        else:
            return f"[code]\n{code}\n[/code]"

    bbcode_text = re.sub(r'```(\w+)?\n([\s\S]*?)\n```', replace_block_code, bbcode_text)

    # 7. Links
    # Convert [text](url) to [url=url]text[/url]
    bbcode_text = re.sub(r'\[(.*?)\]\((.*?)\)', r'[url=\2]\1[/url]', bbcode_text)

    # 8. Lists
    # Convert unordered lists: - Item or * Item to [list][*] Item [/list]
    def replace_unordered_lists(match):
        list_content = match.group(0)
        items = re.findall(r'^\s*[-*]\s+(.*)', list_content, flags=re.MULTILINE)
        bbcode_list = "[list]\n" + "\n".join([f"[*] {item}" for item in items]) + "\n[/list]"
        return bbcode_list

    bbcode_text = re.sub(r'(?:^\s*[-*]\s+.*\n?)+', replace_unordered_lists, bbcode_text, flags=re.MULTILINE)

    # Convert ordered lists: 1. Item to [list=1][*] Item [/list]
    def replace_ordered_lists(match):
        list_content = match.group(0)
        items = re.findall(r'^\s*\d+\.\s+(.*)', list_content, flags=re.MULTILINE)
        bbcode_list = "[list=1]\n" + "\n".join([f"[*] {item}" for item in items]) + "\n[/list]"
        return bbcode_list

    bbcode_text = re.sub(r'(?:^\s*\d+\.\s+.*\n?)+', replace_ordered_lists, bbcode_text, flags=re.MULTILINE)

    # 9. Blockquotes
    # Convert > Quote to [quote]Quote[/quote]
    def replace_blockquotes(match):
        quote = match.group(1)
        return f"[quote]{quote.strip()}[/quote]"

    bbcode_text = re.sub(r'^>\s?(.*)', replace_blockquotes, bbcode_text, flags=re.MULTILINE)

    # 10. Horizontal Rules
    # Convert --- or *** or ___ to [hr]
    bbcode_text = re.sub(r'^(\*\*\*|---|___)$', r'[hr]', bbcode_text, flags=re.MULTILINE)

    # 11. Line Breaks
    # Convert two or more spaces at the end of a line to [br]
    bbcode_text = re.sub(r' {2,}\n', r'[br]\n', bbcode_text)

    return bbcode_text

def parse_arguments():
    """
    Parses command-line arguments.

    Returns:
        argparse.Namespace: Parsed arguments containing input file path, BBCode type, repo name, and output folder.
    """
    parser = argparse.ArgumentParser(
        description='Convert a Markdown file to BBCode format.',
        epilog='Example usage: python markdown_to_bbcode.py input.md --type nexus --repo username/repo --output-folder ./output'
    )
    parser.add_argument('input_file', help='Path to the input Markdown file.')
    parser.add_argument('-t', '--type', choices=['egosoft', 'nexus'], default='egosoft',
                        help='Type of BBCode format to use (default: egosoft).')
    parser.add_argument('-r', '--repo', help='GitHub repository name (e.g., username/repo) to generate absolute image URLs.', default=None)
    parser.add_argument('-o', '--output-folder', help='Path to the output folder. Defaults to the current directory.', default='.')

    return parser.parse_args()

def generate_output_filename(input_file, bbcode_type, output_folder):
    """
    Generates the output file name based on the input file, BBCode type, and output folder.

    Args:
        input_file (str): Path to the input Markdown file.
        bbcode_type (str): Type of BBCode format ('egosoft' or 'nexus').
        output_folder (str): Path to the output folder.

    Returns:
        str: Generated output file path.
    """
    base = os.path.splitext(os.path.basename(input_file))[0]
    output_filename = f"{base}.{bbcode_type}"
    return os.path.join(output_folder, output_filename)

def get_repo_name():
    """
    Retrieves the GitHub repository name from the Git configuration.

    Returns:
        str: The repository name in the format 'username/repo'.
    """
    try:
        repo_url = subprocess.check_output(['git', 'config', '--get', 'remote.origin.url'], encoding='utf-8').strip()
        if repo_url.startswith('https://github.com/'):
            repo_name = repo_url[len('https://github.com/'):].rstrip('.git')
            return repo_name
        elif repo_url.startswith('git@github.com:'):
            repo_name = repo_url[len('git@github.com:'):].rstrip('.git')
            return repo_name
        else:
            print(f"Error: Unsupported repository URL format: {repo_url}")
            sys.exit(1)
    except subprocess.CalledProcessError as e:
        print(f"Error: Unable to retrieve repository URL: {e}")
        sys.exit(1)

def main():
    args = parse_arguments()

    input_path = args.input_file
    bbcode_type = args.type
    repo_name = args.repo or get_repo_name()
    output_folder = args.output_folder

    # Check if input file exists
    if not os.path.isfile(input_path):
        print(f"Error: The input file '{input_path}' does not exist.")
        sys.exit(1)

    # Create output folder if it doesn't exist
    if not os.path.isdir(output_folder):
        try:
            os.makedirs(output_folder)
            print(f"Created output directory: '{output_folder}'")
        except Exception as e:
            print(f"Error creating output directory '{output_folder}': {e}")
            sys.exit(1)

    try:
        with open(input_path, 'r', encoding='utf-8') as infile:
            markdown_content = infile.read()
    except Exception as e:
        print(f"Error reading the input file: {e}")
        sys.exit(1)

    # Extract relative path part from input_path
    relative_path = os.path.dirname(input_path)

    # Convert Markdown to BBCode
    bbcode_result = convert_markdown_to_bbcode(markdown_content, repo_name=repo_name, bbcode_type=bbcode_type, relative_path=relative_path)

    # Generate output file name
    output_path = generate_output_filename(input_path, bbcode_type, output_folder)

    try:
        with open(output_path, 'w', encoding='utf-8') as outfile:
            outfile.write(bbcode_result)
        print(f"Successfully converted '{input_path}' to '{output_path}'.")
    except Exception as e:
        print(f"Error writing to the output file: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()