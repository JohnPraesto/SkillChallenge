USER

int		UserId
string	UserName
string	Password 
string	ProfilePicture

-----------------

Challenge

int				ChallengeId
string			ChallengeName
Date			TimePeriod
[Users]			Users
string			Description
bool			IsPublic
UnderCategory	UnderCategory

-----------------

Category

int					CategoryId
string				CategoryName
[UnderCategories]	UnderCategories

-----------------

UnderCategory

int UnderCategoryId
string UnderCategoryName
int categoryId
Category Category
