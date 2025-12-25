# Cloud Pricing API Setup Guide

This guide explains how to configure API keys for live cloud pricing data in the Infrastructure Sizing Calculator.

## Overview

By default, the calculator uses cached pricing data that's updated periodically. For more accurate, real-time pricing estimates, you can configure API keys to fetch live pricing data from cloud providers.

| Provider | API Type | Auth Required | Rate Limits |
|----------|----------|---------------|-------------|
| AWS | Pricing API | Yes (IAM) | 10 requests/second |
| Azure | Retail Prices API | No (Public) | 100 requests/minute |
| GCP | Cloud Billing API | Yes (Service Account) | 100 requests/minute |

---

## AWS Pricing API

### Prerequisites
- AWS Account with appropriate permissions
- AWS CLI configured (optional, for testing)

### Step 1: Create IAM Policy

Create a custom IAM policy with the minimum required permissions:

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "pricing:GetProducts",
                "pricing:DescribeServices",
                "pricing:GetAttributeValues"
            ],
            "Resource": "*"
        }
    ]
}
```

Save this as `pricing-api-policy.json`.

### Step 2: Create IAM User

1. Go to **AWS Console > IAM > Users**
2. Click **Add users**
3. Enter a name like `pricing-api-user`
4. Select **Access key - Programmatic access**
5. Attach the policy created in Step 1
6. Complete the wizard and save the credentials

### Step 3: Generate Access Keys

1. In IAM > Users, select the user
2. Go to **Security credentials** tab
3. Click **Create access key**
4. Choose **Application running outside AWS**
5. Download and securely store the keys

### Step 4: Configure in Settings

Navigate to **Settings > Cloud Pricing Cache > API Key Configuration** and enter:
- **Access Key ID**: `AKIA...`
- **Secret Access Key**: `wJalrXUtnFEMI/K7MDENG/...`

### Testing (Optional)

```bash
aws pricing get-products \
    --service-code AmazonEC2 \
    --filters "Type=TERM_MATCH,Field=instanceType,Value=m5.large" \
    --region us-east-1 \
    --max-results 1
```

### Troubleshooting

| Issue | Solution |
|-------|----------|
| `AccessDeniedException` | Check IAM policy is attached correctly |
| `ThrottlingException` | Reduce request frequency (max 10/sec) |
| Empty results | Pricing API only works in `us-east-1` region |

---

## Azure Retail Prices API

### Overview

Azure Retail Prices API is **publicly accessible** and doesn't require authentication. The calculator uses this API automatically when available.

### API Endpoint

```
https://prices.azure.com/api/retail/prices
```

### Sample Query

```bash
curl "https://prices.azure.com/api/retail/prices?\$filter=serviceName eq 'Virtual Machines' and armRegionName eq 'eastus' and priceType eq 'Consumption'"
```

### Rate Limits

- **100 requests per minute** per IP address
- Responses are paginated (100 items per page)

### Troubleshooting

| Issue | Solution |
|-------|----------|
| `429 Too Many Requests` | Wait and retry, or reduce request frequency |
| Slow responses | Filter queries to reduce response size |
| Stale data | Azure updates prices daily; cache refreshes every 24h |

---

## GCP Cloud Billing API

### Prerequisites
- GCP Project with billing enabled
- Cloud Billing API enabled
- Service Account with appropriate permissions

### Step 1: Enable the API

1. Go to **GCP Console > APIs & Services > Library**
2. Search for "Cloud Billing API"
3. Click **Enable**

### Step 2: Create Service Account

1. Go to **IAM & Admin > Service Accounts**
2. Click **Create Service Account**
3. Name: `pricing-api-sa`
4. Description: "Service account for pricing data access"
5. Click **Create and Continue**

### Step 3: Assign Roles

Add these roles to the service account:
- `Billing Account Viewer` (for pricing catalog access)
- `Cloud Billing Catalog Viewer` (optional, for SKU details)

### Step 4: Generate JSON Key

1. Click on the service account
2. Go to **Keys** tab
3. Click **Add Key > Create new key**
4. Select **JSON** format
5. Download and securely store the key file

### Step 5: Configure in Settings

Navigate to **Settings > Cloud Pricing Cache > API Key Configuration** and paste the entire JSON key file contents into the **Service Account JSON** field.

### Sample JSON Key Structure

```json
{
    "type": "service_account",
    "project_id": "your-project-id",
    "private_key_id": "key-id",
    "private_key": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n",
    "client_email": "pricing-api-sa@your-project.iam.gserviceaccount.com",
    "client_id": "123456789",
    "auth_uri": "https://accounts.google.com/o/oauth2/auth",
    "token_uri": "https://oauth2.googleapis.com/token"
}
```

### Testing (Optional)

```bash
# Set credentials
export GOOGLE_APPLICATION_CREDENTIALS="/path/to/key.json"

# Test API access
curl -H "Authorization: Bearer $(gcloud auth print-access-token)" \
    "https://cloudbilling.googleapis.com/v1/services"
```

### Troubleshooting

| Issue | Solution |
|-------|----------|
| `PERMISSION_DENIED` | Verify service account has correct roles |
| `API_DISABLED` | Enable Cloud Billing API in console |
| Invalid JSON | Ensure the entire key file is pasted correctly |

---

## Security Best Practices

1. **Least Privilege**: Only grant minimum required permissions
2. **Rotate Keys**: Rotate API keys every 90 days
3. **Monitor Usage**: Set up billing alerts for unexpected API usage
4. **Don't Commit Keys**: Never commit API keys to version control
5. **Use Secrets Manager**: Consider using cloud secrets managers for production

---

## Caching Strategy

The calculator implements intelligent caching to minimize API calls:

| Cache Type | Duration | Trigger |
|------------|----------|---------|
| Pricing Data | 24 hours | Auto-refresh or manual reset |
| Region List | 7 days | On startup |
| SKU Catalog | 7 days | On startup |

### Manual Cache Refresh

1. Go to **Settings > Cloud Pricing Cache**
2. Click **Reset to Defaults** to clear cache and reload default pricing
3. With API keys configured, live data will be fetched on next calculation

---

## Fallback Behavior

If API calls fail, the calculator gracefully falls back to cached default pricing:

```
Live API → Local Cache → Default Pricing Data
```

This ensures calculations always complete, even without network access.

---

## Support

For issues with the pricing integration:

1. Check the [Troubleshooting](#troubleshooting) section for each provider
2. Verify API keys are correctly configured in Settings
3. Check browser console for error messages
4. Report issues at: https://github.com/anthropics/claude-code/issues
