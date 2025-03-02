1. get Lates code from the Repo
2. Restore the Nuget Package
3. Run the Application
4. Use Swagger to get Access Token with Role
	Users List
		userName :Admin
		password : Admin
		rRole: Admin

		userName :User
		password : User 
		Role: User
5. API Access with User Role:
		These will be accssed by using Admin user
			api/CurrencyConversion/latest/v1 
			api//CurrencyConversion/history/v1	 
		This will be Accessed by using normal User (userName:User)
		    api/CurrencyConversion/rate/v1
6. I have added the Fluent Validation but not used Yet but I know how to use it.

Please go through the code and Let's connect for further discussion on this code.
