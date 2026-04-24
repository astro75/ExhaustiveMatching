## Release 0.5.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
EM0001 | Logic | Error | Enum value not handled by switch
EM0002 | Logic | Error | Null value not handled by nullable enum switch
EM0003 | Logic | Error | Closed subtype not handled by switch
EM0004 | Logic | Error | Null value not handled by nullable closed type switch
EM0011 | Logic | Error | Concrete subtype must be a closed type case
EM0012 | Logic | Error | Closed type case must be a direct subtype
EM0013 | Logic | Error | Closed type case must be a subtype
EM0014 | Logic | Error | Concrete subtype must be covered by a closed type case
EM0015 | Logic | Error | Open interface subtype must be a closed type case
EM0100 | Logic | Error | When guard is not supported in exhaustive switch
EM0101 | Logic | Error | Case pattern is not supported in exhaustive switch
EM0102 | Logic | Error | Exhaustive switch target must be enum or closed type
EM0103 | Logic | Error | Match must be on a case type
EM0104 | Logic | Error | Duplicate Closed attribute
EM0105 | Logic | Error | Duplicate case type

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
