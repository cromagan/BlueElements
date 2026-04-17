# Syntax-Änderungen für Modernisierung

### JoinWith -> string.Join
**Alt:**
x.JoinWith("joinchar")
werte.JoinWith("; ")
list.JoinWithCr()

**Neu:**
string.Join('joinchar', x)
string.Join('; ', werte)
string.Join('\r', list)

### ContainsIgnoreCase -> IndexOf
**Alt:**
text.ContainsIgnoreCase(search)
source.ContainsIgnoreCase(toCheck)

**Neu:**
source?.Contains(toCheck, StringComparison.OrdinalIgnoreCase) 
text?.Contains(search, StringComparison.OrdinalIgnoreCase) 

### ContainsWord -> IndexOfWord
**Alt:**
input.ContainsWord(value, RegexOptions.IgnoreCase)
text.ContainsWord(searchTerm, options)

**Neu:**
input.IndexOfWord(value, 0, options) >= 0
text.IndexOfWord(searchTerm, 0, options) >= 0

### JoinWithCr mit Längenbegrenzung
**Alt:**
x.JoinWithCr()
x.JoinWithCr(maxLength)


**Neu:**
string.Join('\r', x)
string.Join('\r', x.TakeWhile(s => result.Length + s.Length <= maxLength))
