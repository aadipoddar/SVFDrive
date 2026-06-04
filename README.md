# SVFDrive
Drive Management for SVF

Latest Version = 1.0.1.4

---

## Deploying an updated FileExplorerAPI (Linux server)

The file-system API runs on the Linux box `svfmni` (LAN `10.10.10.68`, SELinux **Enforcing**)
as the systemd service `fileexplorerapi` (user `mni_api`, port `5033`). The **live** app is at
`/opt/fileexplorerapi` — the `~/fileexplorerapi*` folders in the home dir are only upload staging.

**1. Build (Windows, in repo root):**
```powershell
dotnet publish FileExplorerAPI/FileExplorerAPI.csproj -c Release -r linux-x64 --self-contained true -o FileExplorerAPI/bin/publish-linux
```

**2. Upload to the server (enter the `mni_api` password when prompted):**
```powershell
scp -r FileExplorerAPI/bin/publish-linux mni_api@10.10.10.68:~/fileexplorerapi-new
```

**3. SSH in (`ssh mni_api@10.10.10.68`) and swap it into place:**
```bash
sudo systemctl stop fileexplorerapi && \
sudo cp -rf ~/fileexplorerapi-new/* /opt/fileexplorerapi/ && \
sudo chmod +x /opt/fileexplorerapi/FileExplorerAPI && \
sudo restorecon -Rv /opt/fileexplorerapi >/dev/null && \
sudo systemctl start fileexplorerapi && \
sleep 2 && systemctl is-active fileexplorerapi && \
curl -s -o /dev/null -w "HTTP %{http_code}\n" http://localhost:5033/
```
Success = `active` plus an HTTP code (200/404 both fine).

**4. Clean up the home staging folders** (optional but tidy — these are *not* the live app, so deleting them is safe; no `sudo` needed):
```bash
rm -rf ~/fileexplorerapi ~/fileexplorerapi-new
```
`~/fileexplorerapi-new` is the build you just uploaded; `~/fileexplorerapi` is an old stale copy.
Only `/opt/fileexplorerapi` is live, so removing both home folders does not affect the running service.

**Don't skip these — each one breaks the deploy if missed:**
- `--self-contained true` — the server has no .NET runtime; the publish must carry its own.
- `chmod +x` — scp from Windows drops the executable bit, so the binary won't start.
- `restorecon` — SELinux is Enforcing; without restoring the context the binary is blocked from exec even after `chmod`.
- Deploy into `/opt/fileexplorerapi`, **not** the home folders.

> ⚠️ **API only.** This ships the API alone. UI changes (buttons/pages/JS, e.g. file Preview) live
> in the web app (`svfdrive` App Service) and the MAUI app, which deploy separately via the
> push-to-`main` pipeline with the version bumps described in `CLAUDE.md`.