using MediatR;
using Domain;
using Application.Core;
using Microsoft.AspNetCore.Http;
using Application.Interfaces;
using Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Photos
{
    public class Add
    {
        public class Command : IRequest<Result<Photo>>
        {
            public IFormFile File { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Photo>>
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

            public async Task<Result<Photo>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users.Include(p => p.Photos)
                    .FirstOrDefaultAsync(x => x.UserName == _userAccesor.GetUserName());

                if (user == null) return null;

                var photoUploadResult = await _photoAccesor.AddPhoto(request.File);

                var photo = new Photo
                {
                    Url = photoUploadResult.Url,
                    Id = photoUploadResult.PublicId
                };

                if (!user.Photos.Any(x => x.IsMain)) photo.IsMain = true;

                user.Photos.Add(photo);

                var result = await _context.SaveChangesAsync() > 0;

                if (result) return Result<Photo>.Success(photo);

                return Result<Photo>.Failure("Problem adding photo");
            }
        }
    }
}