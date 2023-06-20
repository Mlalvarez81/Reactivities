using MediatR;
using Domain;
using Application.Core;
using Microsoft.AspNetCore.Http;
using Application.Interfaces;
using Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Photos
{
    public class Delete
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IPhotoAccesor _photoAccesor;
            private readonly IUserAccessor _userAccesor;

            public Handler(DataContext context, IPhotoAccesor photoAccesor, IUserAccessor userAccesor)
            {
                this._context = context;
                this._photoAccesor = photoAccesor;
                this._userAccesor = userAccesor;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users.Include(p => p.Photos)
                    .FirstOrDefaultAsync(x => x.UserName == _userAccesor.GetUserName());

                if (user == null) return null;


                var photo = user.Photos.FirstOrDefault(x => x.Id == request.Id);

                if (photo == null) return null;

                if (photo.IsMain) return Result<Unit>.Failure("Yo cannot delete your main photo");

                var result = await _photoAccesor.DeletePhoto(photo.Id);

                if (result == null) return Result<Unit>.Failure("Problem deleting photo from Cloudinary");

                user.Photos.Remove(photo);

                var success = await _context.SaveChangesAsync() > 0;

                if (success) return Result<Unit>.Success(Unit.Value);

                return Result<Unit>.Failure("Problem deleting photo from API");
            }
        }
    }
}