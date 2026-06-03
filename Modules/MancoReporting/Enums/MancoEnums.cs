namespace IntegrationGateway.Api.Modules.MancoReporting.Enums;

public enum UserRole { TeamMember, Manco, Admin }

public enum ProjectStatus { Draft, Active, Backlogged, Completed, OnHold }

public enum ProjectPriority { Unset, Critical, High, Medium, Low, Deferred }

public enum TaskStatus { Todo, InProgress, Blocked, InReview, Done }

public enum TaskPriority { Critical, High, Medium, Low }

public enum ReportStatus { Draft, Submitted, UnderReview, Actioned, Archived }

public enum CommentType { General, Priority, Concern, Approval, Question, Directive }

public enum PriorityLevel { Critical, High, Medium, Low, Deferred }
