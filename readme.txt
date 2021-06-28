Luke Smith S3807720  -- SOLO WORK --
Git: https://github.com/rmit-wdt-sp2-2021/s3807720-a1

Observer Pattern
Implemented in DatabaseObserver, Menu(addcustomerdatatodatabase method) & AccountManager(addTransaction method) 
I implemented this pattern so I could easily call a single method to add transactions or update the account in the database when needed, such as
adding a new transaction which required these to be updated. Another benefit is the ease of updating the method later should I need to.
This also allows easier integration with a GUI interactions, which may be needed for the web version?
I could have also used this to read account data from the database instead of constantly updating the program files, this may add more security,
however I didn't get to this.

Facade Pattern
This has been implemented to some extent in the menu and accountmanager. 
Appropriate requests are tossed off to the appropriate class(such as building transactions in the transactionmanager).
The benefit of this is it's easier to maintain the code, as any logical issues can be narrowed down to a single class, and assists with splitting
tasks within a group.

Async
The only part of the current version that benefits from async and await is the database-related functions, such as updating account balance or adding in transactions.
By making these asynchronous, the application doesn't need to hold up even if the internet connection is slow. 
If there were a large amount of accounts to go through, that would have benefited significantly from using this too.

Class Library
I only used two methods and a static connection string for this, I could not find any other uses except for perhaps grabbing data from the database,
which would make this library non-reusable anyway.
I used this to hide the input text for logins, which is reusable in the future.
And to grab and return a data table. This could be reused for most other database-linked projects.