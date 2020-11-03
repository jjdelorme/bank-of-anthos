# Building and developing with the overdraft service

## Configuration

Download json security key for your project and reference it below.

Ensure you have a launch.json file configured (.vscode directory) which contains these environment variables, see Google Cloud SDK authentication docs for details.

```json
   "configurations": [
        {
            "name": ".NET Core Launch (web)",
            ...
            },
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development",
                "GOOGLE_APPLICATION_CREDENTIALS": "anthos-sa.json",
                "GOOGLE_PROJECT_ID": "jasondel-test-project",  
            },
            ...
```
