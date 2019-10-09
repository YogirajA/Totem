# Totem Examples

### What is the SalesOrderApp?

This is an example project which uses the Totem API to test messages against an already defined contract.  
- A seed script (placed in examples/Messaging/up) was used to add a sample Sales Order contract to the Contracts table in the Totem-DEV Database. The contract string for the sample contract can be found below:
```
{
   "Contract":{
      "type":"object",
      "properties":{
         "Id":{
            "$ref":"#/Guid",
            "example":"01234567-abcd-0123-abcd-0123456789ab"
         },
         "Timestamp":{
            "type":"string",
            "format":"date-time",
            "example":"2019-01-01T18:14:29Z"
         },
         "ItemName":{
            "type":"string",
            "example":"sample string"
         },
         "OrderDate":{
            "type":"string",
            "format":"date-time",
            "example":"2019-01-01T18:14:29Z"
         },
         "OrderId":{
            "type":"string",
            "example":"sample string"
         }
      }
   },
   "Guid":{
      "type":"string",
      "pattern":"^(([0-9a-f]){8}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){12})$",
      "minLength":36,
      "maxLength":36,
      "example":"01234567-abcd-0123-abcd-0123456789ab"
   }
}
```

### Running the example SalesOrderApp against a Totem instance

Following are the steps to run the example SalesOrderApp in order to test the validity of messages as a producer/consumer through the Totem API: 

- Clone the Totem repo at `https://github.com/HeadspringLabs/Totem.git`
- Using a command prompt from the project folder run '.\build testexample' to run the build script, initialize the DB with the sample contract, and run the SalesOrderApp. 
- The tests in the SalesOrderApp will make a call to the Totem API with the contract ID and sample message. The Totem app configuration settings are given in SalesOrderApp/appsetting.json which by default has the following settings:
```
{
  "TotemSettings": {
    "TestMessageApiUrl": "https://localhost:5001/api/TestMessage",
    "ValidPlaceOrderContractId": "7820D508-26F6-4E9E-A4D6-EBD3EF7ED47B"
  }
}
```