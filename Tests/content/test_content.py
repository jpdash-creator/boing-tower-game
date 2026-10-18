"""Run from the repo root:  python -m unittest discover -s Tests/content -v"""
import os
import sys
import unittest

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "Tools", "content_cli")))
import content  # noqa: E402


class SchemaSubset(unittest.TestCase):
    def errors(self, value, schema):
        out = []
        content.check_schema(value, schema, "$", out)
        return out

    def test_required_and_additional(self):
        schema = {"type": "object", "required": ["a"], "additionalProperties": False,
                  "properties": {"a": {"type": "integer"}}}
        self.assertEqual(self.errors({"a": 1}, schema), [])
        self.assertTrue(self.errors({}, schema))
        self.assertTrue(self.errors({"a": 1, "b": 2}, schema))

    def test_types_and_bounds(self):
        self.assertTrue(self.errors(True, {"type": "integer"}))
        self.assertTrue(self.errors(1.5, {"type": "integer"}))
        self.assertTrue(self.errors(5, {"type": "integer", "maximum": 3}))
        self.assertEqual(self.errors(None, {"type": ["string", "null"]}), [])

    def test_pattern_and_enum(self):
        self.assertTrue(self.errors("x", {"type": "string", "pattern": "^y"}))
        self.assertTrue(self.errors("c", {"enum": ["a", "b"]}))


class ShippedContent(unittest.TestCase):
    def test_validates_clean(self):
        errors, regions, levels = content.validate()
        self.assertEqual(errors, [])
        self.assertEqual((regions, levels), (30, 450))

    def test_gem_skip_formula(self):
        self.assertIsNone(content.expected_gem_skip(1))
        self.assertEqual(content.expected_gem_skip(2), 100)
        self.assertEqual(content.expected_gem_skip(20), 400)
        self.assertEqual(content.expected_gem_skip(21), 416)


if __name__ == "__main__":
    unittest.main()
