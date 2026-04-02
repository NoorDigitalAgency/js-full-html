namespace JsFullHtml;

/// <summary>
/// Provides a JavaScript snippet for retrieving the full serialized HTML of a page,
/// including shadow DOM content, for use with browser drivers such as Playwright.
/// </summary>
public static class JsFullHtml
{
    private const string Script = """
                                  (() => {
                                      function escapeAttr(str) {
                                          return str.replace(/&/g, '&amp;').replace(/"/g, '&quot;');
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
                                          const openTag = /<template shadowrootmode="[^"]*">/g;
                                          let result = '';
                                          let lastIndex = 0;
                                          let match;
                                          while ((match = openTag.exec(html)) !== null) {
                                              result += html.slice(lastIndex, match.index);
                                              let depth = 1;
                                              let i = openTag.lastIndex;
                                              const templateOpen = /<template[\s>]/g;
                                              const templateClose = /<\/template>/g;
                                              while (depth > 0 && i < html.length) {
                                                  templateOpen.lastIndex = i;
                                                  templateClose.lastIndex = i;
                                                  const nextOpen = templateOpen.exec(html);
                                                  const nextClose = templateClose.exec(html);
                                                  if (!nextClose) break;
                                                  if (nextOpen && nextOpen.index < nextClose.index) {
                                                      depth++;
                                                      i = nextOpen.index + nextOpen[0].length;
                                                  } else {
                                                      depth--;
                                                      if (depth === 0) {
                                                          result += html.slice(openTag.lastIndex, nextClose.index);
                                                          lastIndex = nextClose.index + nextClose[0].length;
                                                      } else {
                                                          i = nextClose.index + nextClose[0].length;
                                                      }
                                                  }
                                              }
                                          }
                                          result += html.slice(lastIndex);
                                          return result;
                                      }
                                      const inner = document.documentElement.getHTML({ shadowRoots: getAllShadowRoots(document) });
                                      return `<!DOCTYPE html><html${getAttributes(document.documentElement)}>${flattenShadowTemplates(inner)}</html>`;
                                  })()
                                  """;

    /// <summary>
    /// Gets the full rendered HTML including flattened shadow DOM content.
    /// </summary>
    public static Task<string> GetFullHtmlAsync(Func<string, Task<string>> evaluateAsync)
    {
        return evaluateAsync(Script);
    }
}
