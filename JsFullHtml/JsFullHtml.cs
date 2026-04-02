namespace JsFullHtml;

/// <summary>
/// Provides a JavaScript snippet for retrieving the full serialized HTML of a page,
/// including shadow DOM content, for use with browser drivers such as Playwright.
/// </summary>
public class JsFullHtml
{
    private const string Script = """

                                      (() => {
                                          const rawContentTags = new Set(['script', 'style']);
                                          function escapeHtml(str) {
                                              return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
                                          }
                                          function escapeAttr(str) {
                                              return str.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
                                          }
                                          function getAttributes(el) {
                                              return Array.from(el.attributes)
                                                  .map(a => ` ${a.name}="${escapeAttr(a.value)}"`).join('');
                                          }
                                          function getAllShadowRoots(root) {
                                              const shadows = [];
                                              const walk = (node) => {
                                                  const els = (node.shadowRoot || node).querySelectorAll('*');
                                                  for (const el of els) {
                                                      if (el.shadowRoot) {
                                                          shadows.push(el.shadowRoot);
                                                          walk(el.shadowRoot);
                                                      }
                                                  }
                                              };
                                              walk(root);
                                              return shadows;
                                          }
                                          function flattenShadowTemplates(html) {
                                              const openTag = /<template\b[^>]*\bshadowrootmode\b[^>]*>/gi;
                                              const closeTag = /<\/template>/gi;
                                              return html.replace(openTag, '').replace(closeTag, '');
                                          }
                                          const voidTags = new Set(['area', 'base', 'br', 'col', 'embed', 'hr', 'img', 'input', 'link', 'meta', 'param', 'source', 'track', 'wbr']);
                                          function serialize(node) {
                                              if (node.nodeType === Node.TEXT_NODE) {
                                                  const parentTag = node.parentNode?.tagName?.toLowerCase();
                                                  return rawContentTags.has(parentTag) ? node.nodeValue : escapeHtml(node.nodeValue);
                                              }
                                              if (node.nodeType === Node.COMMENT_NODE) return `<!--${node.nodeValue.replace(/--/g, '- -')}-->`;
                                              if (node.nodeType === Node.DOCUMENT_NODE || node.nodeType === Node.DOCUMENT_FRAGMENT_NODE) {
                                                  return Array.from(node.childNodes).map(serialize).join('');
                                              }
                                              if (node.nodeType !== Node.ELEMENT_NODE) return '';
                                              const tag = node.tagName.toLowerCase();
                                              const attrs = getAttributes(node);
                                              if (voidTags.has(tag)) return `<${tag}${attrs}>`;
                                              let content = '';
                                              if (node.shadowRoot) {
                                                  content += '<template shadowrootmode="open">';
                                                  content += serialize(node.shadowRoot);
                                                  content += '</template>';
                                              }
                                              content += Array.from(node.childNodes).map(serialize).join('');
                                              return `<${tag}${attrs}>${content}</${tag}>`;
                                          }
                                          const doctype = document.doctype ? `<!DOCTYPE ${document.doctype.name}>` : '';
                                          const html = doctype + serialize(document.documentElement);
                                          return flattenShadowTemplates(html);
                                      })();

                                  """;

    /// <summary>
    /// Returns the JavaScript snippet that, when evaluated in a browser context, serializes
    /// the full DOM including all shadow roots into a single HTML string.
    /// </summary>
    /// <returns>A JavaScript expression string suitable for use with browser evaluation APIs.</returns>
    public static string GetScript() => Script;
}
