## ChatBot
The app consists of a .NET 9 backend ([GithHub](https://github.com/Dulz/ChatBotBE)) and a React frontend ([GithHub](https://github.com/Dulz/chat-bot-fe)).

Simply the frontend uses the backend API to send and receive messages.  
The backend uses the OpenAI API to generate responses to the messages and a CosmosDB to store the chat history.  
Azure AD B2C is used for authentication.

You can find the app running [here](https://lemon-pond-040ee4e00.4.azurestaticapps.net/).

### How to run the app
You will require the local appSettings.json file to run the backend. 
Please contact me for the file if you plan to run the app locally.  
After you add the appSettings.json file simply run the dotnet app:
```bash
dotnet run --project ChatBot
```

### Architecture and Design
You will find three namespaces/folders in the backend:  
- Chat: Hosts the service and domain objects used to interact with the other components (such as History and different ChatProviders).
- ChatProviders: Hosts the different chat providers that can be used to interact with the chatbot. Currently, only the OpenAI provider is implemented.
- ChatHistory: Hosts the different storage options for the chat history. Currently, only the CosmosDB provider is implemented.

The services interact with each other via common interfaces that only use domain objects. While the different Dtos used to communicate with APIs are internal to the namespaces.  
This allows for easy swapping of the different components (i.e. changing the chat provider or the storage provider).

The project is unit tested using xUnit and Moq.

#### Decisions
- CosmosDB was chosen as the storage provider because it is a NoSQL database that can be easily scaled and lends itself well to the chat history data.  
Meaning we are unlikely to need to perform complex queries on the data that would require a relational database.
- OpenAI and ChatGPT are obvious choices due to their popularity and ease of use.
- Azure AD B2C was chosen to keep with the Azure hosting approach. Although in retrospect I have found it to be very cumbersome to configure and a huge time sink. It also doesn't look the best on the frontend.

### Considerations and improvements
- If the app was to be further developed it would be good to have a local setup for OpenAI and CosmosDB. This would allow for easier testing and development.
- Further integration testing for the CosmosDB and OpenAI providers would be beneficial.
- Better error handling and logging. Especially for the OpenAI provider and different responses from the API.
- Pagination of chat history would be good to have for the frontend. When it comes to sending the history to ChatGPT it would be a bit tricky to decide how much history to send to maintain context.
- Validation of the input to the API. Like restricting the number of characters.
- Streaming support.

### Hosting and Deployment
The app is hosted on Azure using Azure App Services.
This approach is very easy to set up and maintain. There are also many options for scaling and monitoring the app.
However, if the scale of the app was to increase it would be beneficial to look into Kubernetes or another container-based solution where you have more control.
The pipeline setup is managed by a GitHub action that builds, tests and deploys the app on push to the main branch.


