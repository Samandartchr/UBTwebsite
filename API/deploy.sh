gcloud builds submit --tag us-central1-docker.pkg.dev/ubtprep-site/cloud-run-source-deploy/api-service:latest .

gcloud run deploy api-service --image us-central1-docker.pkg.dev/ubtprep-site/cloud-run-source-deploy/api-service:latest --region us-central1 --platform managed