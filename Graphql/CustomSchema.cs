﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Graphql
{
    public class CustomSchema : GraphQL.Types.Schema
    {
        public CustomSchema(IServiceProvider provider)
            : base(provider)
        {
            Query = provider.GetRequiredService<CustomQuery>();
            //Mutation = provider.GetRequiredService<StarWarsMutation>();
        }
    }
}
