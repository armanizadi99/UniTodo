namespace UniTodo.Modules.Todos.Domain.Common
{
    public readonly struct Result
    {
        public bool IsSuccess { get; }
        private readonly DomainError _error;
        public DomainError Error => IsSuccess
? throw new InvalidOperationException("Successful results do not contain errors.")
: _error;

        private Result( bool isSuccess, DomainError error )
        {
        IsSuccess = isSuccess;
        _error = error;
        }

        public static Result Success()
        {
        return new Result(true, default(DomainError));
        }

        public static Result Failure( DomainError error )
        {
        return new Result(false, error);
        }

        public static implicit operator Result( DomainError error )
        {
        return Result.Failure(error);
        }
    }

    public readonly struct Result<T>
    {
        public bool IsSuccess { get; }
        private readonly T _value;
        public T Value =>
IsSuccess
? _value
: throw new InvalidOperationException("Failed results do not contain value.");

        private readonly DomainError _error;
        public DomainError Error => IsSuccess
? throw new InvalidOperationException("Successful results do not contain errors.")
: _error;

        private Result( bool isSuccess, T value, DomainError error )
        {
        IsSuccess = isSuccess;
        _value = value;
        _error = error; ;
        }

public static Result<T> Failure( DomainError error ) 
{
        return new Result<T>(false, default(T)!, error);
        }

public static Result<T> Success(T value)
{
        return new Result<T>(true, value, default(DomainError));
        }

public static implicit operator Result<T>(DomainError error)
{
return Result<T>.Failure(error);
        }

public static implicit operator Result<T>(T value)
{
return Result<T>.Success(value);
        }
    }
}