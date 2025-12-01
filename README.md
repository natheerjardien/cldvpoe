# ST10435542 - CLDV6212 - POE - ABC RETAIL WEB APP🛍️

## Overview
A web-based retail management system built with ASP.NET Core MVC that allows users to manage customers, products, orders and files. The system provides CRUD operations for all entities with a user-friendly interface (Deletion will occur from Azure Storage Account to control user access to retail data).
-- https://cldv6212st10435542poe.azurewebsites.net --

## Features✅
- **Customer Management**: Add, view, edit, and delete customer information (First & Last Name; contact; email; password)
- **Product Management**: Create and manage products with detailed information
- **Order System**: Create and manage orders with detailed information that are linked to a specific customer & product
- **File Management**: Upload & Download files - "contacts"

## Screenshots - PART1️⃣
### Login Index View
<img width="1919" height="991" alt="Screenshot 2025-08-27 213558" src="https://github.com/user-attachments/assets/8cfc2fce-d65a-426b-84cf-3d39a45bc0f9" />

### Home Index View
<img width="1919" height="991" alt="Screenshot 2025-08-27 215609" src="https://github.com/user-attachments/assets/49ebd672-cd22-41ba-b699-3d1ee144afee" />

### Customer Index View
<img width="1919" height="989" alt="Screenshot 2025-08-28 200305" src="https://github.com/user-attachments/assets/b4d6511e-9eab-4ebc-b0a6-c930b6f22da2" />

### Product Index View
<img width="1903" height="990" alt="Screenshot 2025-08-28 200319" src="https://github.com/user-attachments/assets/ca82873d-c395-41c5-a629-8bb1da1b95d2" />

### Order Index View
<img width="1903" height="987" alt="Screenshot 2025-08-28 200333" src="https://github.com/user-attachments/assets/90e6068d-7732-42a9-9e2b-bb6b731b5444" />

### File Index View
<img width="1919" height="992" alt="Screenshot 2025-08-27 213752" src="https://github.com/user-attachments/assets/c0d4e993-0e28-40ce-a2c3-08c0da1d42e2" />

## Technologies Used✅
- ASP.NET Core MVC
- NuGet Packages
<img width="1919" height="1079" alt="ST10435542 - CLDV6212 - NuGet Packages installed" src="https://github.com/user-attachments/assets/98ad1ba3-8d48-4e9d-b16b-9fa19f52e584" />
- Azure Storage Account - Tables; Blob Container; File Share; Queue
- Bootstrap - Lux Theme
- HTML/CSS

## Azure Storage
1. Create Storage Account☑️
<img width="825" height="991" alt="ST10435542 - CLDV6212 - Storage Account Basic Settings" src="https://github.com/user-attachments/assets/940983d0-f0a3-40b0-909b-ad865736c5e4" />
<img width="827" height="994" alt="ST10435542 - CLDV6212 - Storage Account Advanced Settings" src="https://github.com/user-attachments/assets/6d74373c-c1db-4e49-ac51-fe894fb7def9" />
<img width="959" height="992" alt="ST10435542 - CLDV6212 - Storage Account Review   Create" src="https://github.com/user-attachments/assets/3a57aaa8-8507-4bf8-8e2e-97ca1811e638" />
<img width="1919" height="993" alt="ST10435542 - CLDV6212 - Storage Account Deployed Successfully" src="https://github.com/user-attachments/assets/2950e7e6-c5bf-4208-88a0-fb6969b65efd" />

2. Create Azure Tables (Customer; Product; Order)☑️
<img width="1919" height="994" alt="ST10435542 - CLDV6212 - Tables Created Successfully" src="https://github.com/user-attachments/assets/2632e072-4b62-42fe-b470-8db19b9e3e05" />

3. Create Azure Blob Container (product)☑️
<img width="1919" height="992" alt="ST10435542 - CLDV6212 - Blob Container deployed successfully" src="https://github.com/user-attachments/assets/fdabc5bc-daab-4137-8b0c-54d65cf9ec5f" />

4. Create Azure Queue (orders)☑️
<img width="1919" height="990" alt="ST10435542 - CLDV6212 - Queue deployed successfully" src="https://github.com/user-attachments/assets/29cf9ce7-3d70-487d-af70-7807e6550031" />

5. Create Azure File Share (fileshare)☑️
<img width="1919" height="968" alt="ST10435542 - CLDV6212 - File Share sucessfully created" src="https://github.com/user-attachments/assets/30ba3783-3ea8-49d6-90af-2c5b5fe83561" />

6. Create Web App Service - Azure
<img width="727" height="988" alt="Screenshot 2025-08-28 192921" src="https://github.com/user-attachments/assets/c396eefe-512b-4c94-88fc-d4fcb565ce8a" />
<img width="748" height="992" alt="Screenshot 2025-08-28 192931" src="https://github.com/user-attachments/assets/54bacefb-db87-4cd9-94ce-8c8050b32c20" />
<img width="709" height="990" alt="Screenshot 2025-08-28 193102" src="https://github.com/user-attachments/assets/011d4b96-b810-479b-bf59-2761e1b53c67" />
<img width="1919" height="964" alt="Screenshot 2025-08-28 193201" src="https://github.com/user-attachments/assets/9171d898-03e5-4647-9124-87039464eaf0" />


## Records Succesfully Added to Azure Storage from fully functioning Razor Views
1. Table Records🗂️
Customer Table
<img width="1919" height="987" alt="ST10435542 - CLDV6212 - Customer Table 5 Records" src="https://github.com/user-attachments/assets/745ef620-1015-4a97-9b87-d07a79c5da5d" />
Product Table
<img width="1919" height="948" alt="ST10435542 - CLDV6212 - Product Table 5 Records" src="https://github.com/user-attachments/assets/952b6364-ada7-4484-8378-2809c609d8eb" />
Order Table
<img width="1919" height="993" alt="ST10435542 - CLDV6212 - Order Table 5 Records" src="https://github.com/user-attachments/assets/6ede602d-5edc-4307-9d0e-a86f144b0c71" />

3. Blob Container Records🗂️
<img width="1919" height="993" alt="ST10435542 - CLDV6212 - Blob Container 5 Records" src="https://github.com/user-attachments/assets/86ea645f-8121-4aea-a199-378ffab033cb" />

4. Queue Records🗂️
<img width="1919" height="991" alt="ST10435542 - CLDV6212 - Queues 5 Records" src="https://github.com/user-attachments/assets/d2b005f3-6602-434f-9deb-d28220dda385" />

5. FileShare Records🗂️
<img width="1918" height="984" alt="ST10435542 - CLDV6212 - File Share 5 Records" src="https://github.com/user-attachments/assets/946ea84d-fb5b-4e65-8ba7-82a54daec05c" />

## Creating the MVC Project
1. Select Project Type, Name Project and choose file location☑️
<img width="927" height="549" alt="Screenshot 2025-08-27 215331" src="https://github.com/user-attachments/assets/49d57fea-b91d-4003-a92c-d48a86d10b9e" />

2. Select Framework Version (8.0)☑️
<img width="1104" height="669" alt="Screenshot 2025-08-27 213243" src="https://github.com/user-attachments/assets/c1099ef2-52dc-469e-a540-5604f5d02829" />

3. Solution Explorer - ALL files
<img width="1919" height="961" alt="Screenshot 2025-08-27 214426" src="https://github.com/user-attachments/assets/1a0406f4-b1d8-4834-a2d8-2bf939037076" />
<img width="1919" height="961" alt="Screenshot 2025-08-27 214440" src="https://github.com/user-attachments/assets/a93afefb-cf0f-4ebc-b609-c21f9b315b4e" />

4. Publish Web App on Visual Studio - Import Profile
<img width="805" height="564" alt="Screenshot 2025-08-28 193304" src="https://github.com/user-attachments/assets/dfd60309-6bd0-4e57-9365-c2c27086ef4f" />
<img width="1919" height="1079" alt="Screenshot 2025-08-28 193327" src="https://github.com/user-attachments/assets/df21f5e2-67c1-4db0-b8bd-265c09f98542" />
<img width="1329" height="867" alt="Screenshot 2025-08-28 193518" src="https://github.com/user-attachments/assets/c90cdb78-7580-4fae-a64b-7e1a9e7d82fd" />

