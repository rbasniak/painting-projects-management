We need to finish implementation of the ColorZonesDialog. Here's what we need:

- The dialog is split into 2 areas: left (image color picker) and right (new things we'll implement in this feature).
- It needs to show the color zones of the picture.
- Color zones are areas that the user will create and then pick the colors for it in the left area. 
- A color zone can have 1 to 3 different colors: highlights, midtones and shadows. 
- When creating a new zone the user will mark checkboxes with the areas he wants for that zone.
- They will be created with neutral grey by default. 
- The user must somehow change those greys for what he picks in the color picker. Not sure how to do that from an UX point of view. First thing comes to my mind is that the user clicks the color he wants to change in the right, then the next color picked in the left will update it.
- UI wise, I was thinking of each zone being an expandable area on the right, then inside each expandable we put the relevant information.
- Edit and delete buttons could be on the aligned to the right in the expandable header and the new action could be a 'New Zone' button somewhere in the right part of the dialog.
- Once the zone is created with a name and available areas, a request is made to the server which persists it.
- Then once a color is updated we do a new request to the server to persist it.
- The edit button should allow for editing the name of the zone and which areas are available.
- Analyse the project and see what is required to be created, both server side and client side. 
- Follow the existing standards from the server modules.
