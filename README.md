# S5LkForm
Lítið kerfi með Azure AD login og tengingu við S5 Leigukerfi.
Hér er hægt að skrá beiðnir eins og þær berast frá Stofnunum.
Stuðningur er fyrir viðhengi t.d. myndir eða verklýsingar.

Athugið að setja þarf inn appsettings.Passwords.json Til að skilgreina 
lykilorð fyrir S5. Dæmi er hér að neðan:

    {
      "S5": {
        "Password": "EitthvertLykilorðSemVirkar"
      }
    }
