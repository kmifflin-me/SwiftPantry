import sqlite3
db = 'c:/Users/Kaelan Mifflin/Documents/SwiftPantry/src/SwiftPantry.Web/Data/swiftpantry_test.db'
conn = sqlite3.connect(db)
c = conn.cursor()
print('UserProfiles:', c.execute('SELECT COUNT(*) FROM UserProfiles').fetchone()[0])
print('PantryItems:', c.execute('SELECT COUNT(*) FROM PantryItems').fetchone()[0])
print('MealLogEntries:', c.execute('SELECT COUNT(*) FROM MealLogEntries').fetchone()[0])
print('Recipes:', c.execute('SELECT COUNT(*) FROM Recipes').fetchone()[0])
print('Profile data:', c.execute('SELECT Id, Age, Goal FROM UserProfiles').fetchall())
conn.close()
