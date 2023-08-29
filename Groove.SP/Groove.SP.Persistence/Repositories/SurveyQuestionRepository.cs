using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Persistence.Repositories
{
    public class SurveyQuestionRepository : Repository<SpContext, SurveyQuestionModel>, ISurveyQuestionRepository
    {
        public SurveyQuestionRepository(SpContext context) : base(context)
        {
        }
    }
}
