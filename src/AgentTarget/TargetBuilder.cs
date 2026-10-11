namespace AgentTarget;

public static class TargetBuilder
{
    public static string Build(string name, string description, string cwd)
    {
        var text = $"{name} {description}".ToLowerInvariant();
        var desktop = text.Contains("desktop") || text.Contains("terminal") || text.Contains("windows app");
        var mobile = text.Contains("mobile") || text.Contains("iphone") || text.Contains("android") || text.Contains("expo");
        var web = text.Contains("web") || text.Contains("dashboard") || text.Contains("website") || text.Contains("platform");
        var known = new[] { desktop, mobile, web }.Count(item => item);
        var target = known == 0 ? "unknown" : known > 1 ? "both" : desktop ? "desktop" : mobile ? "mobile" : "web";
        var wantWeb = target is "web" or "unknown" || (target == "both" && web);
        var project = Path.Combine(cwd, "project", Slug(name));
        var library = Path.Combine(cwd, "library");
        Directory.CreateDirectory(project);
        Directory.CreateDirectory(library);

        File.WriteAllText(Path.Combine(project, "wrangler.toml"), """
name = "app"
pages_build_output_dir = "apps/web"
account_id = "PLACEHOLDER"
""");
        WriteModule(project, library, "cloudflare-host", """
export const id = "cloudflare-host";
const apiToken = process.env.CLOUDFLARE_API_TOKEN || "PLACEHOLDER";
const accountId = process.env.CLOUDFLARE_ACCOUNT_ID || "PLACEHOLDER";
export function run() { return { id, host: "cloudflare-pages", apiToken, accountId, ok: true }; }
""");

        if (desktop || target == "desktop")
        {
            Directory.CreateDirectory(Path.Combine(project, "apps", "desktop"));
            File.WriteAllText(Path.Combine(project, "apps", "desktop", "window.ts"), "export const windowFrame = { width: 1100, height: 720 };\nexport const terminalBoundary = { prompt: \"> \" };\n");
            WriteModule(project, library, "target-desktop", "export const id = \"target-desktop\";\nexport function run() { return { id, target: \"desktop\", ok: true }; }\n");
        }
        if (wantWeb)
        {
            Directory.CreateDirectory(Path.Combine(project, "apps", "web", "src"));
            File.WriteAllText(Path.Combine(project, "apps", "web", "src", "routes.ts"), "export const routes = [{ path: \"/\" }, { path: \"/work\" }, { path: \"/settings\" }];\n");
            File.WriteAllText(Path.Combine(project, "apps", "web", "src", "layout.ts"), "export const layoutTree = { frame: [\"sidebar\", \"topbar\", \"content\"] };\n");
            WriteModule(project, library, "target-web", "export const id = \"target-web\";\nexport function run() { return { id, target: \"web\", host: \"cloudflare\", ok: true }; }\n");
        }
        if (mobile || target == "mobile")
        {
            Directory.CreateDirectory(Path.Combine(project, "apps", "mobile"));
            File.WriteAllText(Path.Combine(project, "apps", "mobile", "stack.ts"), "export const screenStack = [\"Home\", \"Detail\", \"Settings\"];\n");
            WriteModule(project, library, "target-mobile", "export const id = \"target-mobile\";\nconst expoToken = process.env.EXPO_TOKEN || \"PLACEHOLDER\";\nexport function run() { return { id, target: \"mobile\", expoToken, ok: true }; }\n");
        }
        if (target == "unknown")
        {
            WriteModule(project, library, "target-choice", "export const id = \"target-choice\";\nexport const question = \"Is this a desktop app, a website, or a phone app?\";\nexport function run() { return { id, assumed: \"web\", question, ok: true }; }\n");
        }

        var todo = new List<string>
        {
            "Create a free Cloudflare account if you do not have one.",
            "Paste CLOUDFLARE_API_TOKEN and CLOUDFLARE_ACCOUNT_ID later. Both are PLACEHOLDER for now.",
        };
        var missing = new List<string> { "CLOUDFLARE_API_TOKEN", "CLOUDFLARE_ACCOUNT_ID" };
        var questions = new List<string>();
        if (target == "unknown") questions.Add("Is this a desktop app, a website, or a phone app?");
        if (target == "both") questions.Add("Which platform should be finished first?");
        if (mobile) missing.Add("EXPO_TOKEN");

        return string.Join(Environment.NewLine, new[]
        {
            $"Target: {target}. Hosting: Cloudflare Pages.",
            "",
            "What you need to do",
            string.Join(Environment.NewLine, todo.Select(item => "- " + item)),
            "",
            "What we're missing",
            string.Join(Environment.NewLine, missing.Select(item => "- " + item)),
            "",
            "Questions",
            questions.Count == 0 ? "- None. The description names the platform." : string.Join(Environment.NewLine, questions.Select(item => "- " + item)),
        });
    }

    private static void WriteModule(string project, string library, string id, string body)
    {
        var moduleDir = Path.Combine(project, "packages", "modules");
        Directory.CreateDirectory(moduleDir);
        Directory.CreateDirectory(library);
        File.WriteAllText(Path.Combine(moduleDir, id + ".ts"), body);
        File.WriteAllText(Path.Combine(library, id + ".ts"), body);
    }

    private static string Slug(string value)
    {
        var chars = value.Trim().ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = new string(chars).Trim('-');
        return slug.Length == 0 ? "project" : slug;
    }
}
