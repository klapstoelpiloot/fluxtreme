import requests
from enum import Enum
from bs4 import BeautifulSoup
from urllib.parse import urljoin, urlparse

class ParseMode(Enum):
	BeforeArg = 0
	Arg = 1
	Fn = 2

# Crawls all links on the given page and calls process_page for each link
def process_all_pages(url):
	# File to write the output to
	with open("FluxFunctions.txt", 'w') as file:
		# Load the main page
		response = requests.get(url)
		soup = BeautifulSoup(response.text, "html.parser")
		
		# The list of functions is in a <ul> tag with the class "function-list"
		listtag = soup.find("ul", class_="function-list")
		for atag in listtag.find_all("a", href=True):
			link = atag["href"]
			text = atag.text
			full_url = urljoin(url, link)
			try:
				function_def = process_page(full_url, text.rstrip('()'))
				file.write(f"{function_def}\n")
			except Exception as e:
				print(f" - ERROR: {e}")
				print(f"While parsing: {full_url}")

# Processes the given URL and returns the function definition with a simple list of arguments as a string
def process_page(url, functionname):
	print(f"{functionname}", end='')
	response = requests.get(url)
	soup = BeautifulSoup(response.text, "html.parser")

	# Find where the function definition starts
	function_sig_header = soup.find(id="function-type-signature")
	if (function_sig_header == None):
		raise Exception("Function type signature header not found")
	function_sig = function_sig_header.find_next("code")
	if (function_sig == None):
		raise Exception("Function type signature code block not found")

	# Some definitions are on a single line while others are multi-line
	# but the nice thing is that the page uses <span> tags for every
	# token in the definition, so we can use that to make parsing easier.
	args = "("
	mode = ParseMode.BeforeArg
	all_tokens = function_sig.find_all("span")
	if (all_tokens == None):
		raise Exception("No tags found in the code block")
	for token in all_tokens:
		token_classes = token.get("class")
		if (token_classes == None):
			continue
		token_class = token_classes[0]
		if ((token_class == "line") | (token_class == "cl")):
			continue
		if (mode == ParseMode.BeforeArg):
			if ")" in token.text:
				break
			if (token_class == "nx"):
				if not args.endswith("("):
					args += ", "
				args += token.text.strip()
				mode = ParseMode.Arg
		elif (mode == ParseMode.Arg):
			if ")" in token.text:
				break
			if ((token_class == "p") & token.text.endswith(",")):
				mode = ParseMode.BeforeArg
			if ((token_class == "p") & token.text.startswith("(")):
				mode = ParseMode.Fn
		elif (mode == ParseMode.Fn):
			if ((token_class == "p") & token.text.endswith(")")):
				mode = ParseMode.Arg

	args += ")"
	print(f"{args}")
	return f"{functionname}{args}"

if (__name__ == "__main__"):
	main_url = "https://docs.influxdata.com/flux/v0/stdlib/all-functions/"
	process_all_pages(main_url)
	#process_page("https://docs.influxdata.com/flux/v0/stdlib/universe/aggregatewindow/", "aggregatewindow")
