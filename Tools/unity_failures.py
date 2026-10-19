"""Prints the failed test cases (name and message) of a Unity NUnit result XML. Usage: python unity_failures.py <xml> [max_chars]"""
import re
import sys

xml = sys.argv[1]
limit = int(sys.argv[2]) if len(sys.argv) > 2 else 5000
text = open(xml, encoding="utf-8").read()
for m in re.finditer(r'<test-case[^>]*?\sname="([^"]+)"[^>]*?result="Failed"[^>]*>(.*?)</test-case>', text, re.S):
    msg = re.search(r"<message><!\[CDATA\[(.*?)\]\]>", m.group(2), re.S)
    print("== FAILED", m.group(1))
    print((msg.group(1) if msg else m.group(2))[:limit])
