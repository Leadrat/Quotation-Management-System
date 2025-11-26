# Sample Files for Import Templates (Spec 029)

Use these when testing the import flow:

- docx/invoice-sample.docx
- xlsx/items-sample.xlsx
- pdf/invoice-scan.pdf (may require manual mapping)

Testing steps
1. Go to /imports/new and upload one of the samples.
2. Review SuggestedMappings in the session page.
3. Use the Chat Assistant to refine mappings.
4. Edit and Save mappings.
5. Generate preview and open it.
6. Save as template.

Notes
- For PDFs without text layer, mappings may be minimal; use chat + manual edits.
- Ensure company.name and customer.name are set to pass validation.
