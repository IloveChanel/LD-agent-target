# Target Platform

Third agent for the Multi-Agent Builder. Open `LD-agent-target.sln` in Visual Studio 2022 and press F5.

It reads the project description and scaffolds one shape:

- Desktop: a window frame and a terminal boundary.
- Web: page routes and a layout tree.
- Mobile: a screen stack and views.

Cloudflare Pages is the host for now because the free plan is enough. Unknown keys stay as `process.env.CLOUDFLARE_API_TOKEN || "PLACEHOLDER"`. The output lists what you need to do, what is missing, and any question. If the platform is not named, it asks and still saves a web scaffold plus a Target Choice module.

Each new module is saved in the project folder and in the library folder.

```text
dotnet run --project src/AgentTarget -- "Notes" "A web notes dashboard"
```
