# Solution Structure Rules (always loaded)

- Dependency direction is one-way: `Api → Data → Core`. Core references no other project. If a change would require Core to know about Data or Api types, the design is wrong — stop and propose an interface in Core instead.
- New interfaces for services/repositories are declared in Core; implementations live in Data (persistence) or Api (composition).
- DI registrations live in per-project `ServiceCollectionExtensions` (`AddCoreServices()`, `AddDataServices()`), composed in `Program.cs`.
- New projects require justification; prefer folders within existing projects until a real boundary emerges.
- Shared constants and options types go in Core using the Options pattern (`IOptions<T>` bound from configuration), never static config reads.
