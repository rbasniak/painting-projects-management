namespace PaintingProjectsManagement.Features.Projects;

public class ChainedQuery  
{
    public class Request : IQuery<Response>
    {
        public string Data { get; set; }
    } 

    public class Handler : IQueryHandler<Request, Response>
    {
        public async Task<QueryResponse<Response>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            return QueryResponse<Response>.Success(new Response {  Result = request.Data.ToUpper()});
        }
    }

    public class Response
    {
        public string Result { get; set; }
    }
}
