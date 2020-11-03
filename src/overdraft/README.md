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

To run the project locally you need access to the following services.  Use kubectl to forward them locally:

```bash
kubectl port-forward service/ledgerwriter 8080:8080 &
kubectl port-forward service/balancereader 8081:8080 &
kubectl port-forward service/userservice 8084:8080 &
```