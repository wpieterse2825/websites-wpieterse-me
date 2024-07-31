using System;
using System.Collections.Generic;

using Pulumi;
using Gcp = Pulumi.Gcp;
using GitLab = Pulumi.GitLab;
using GitHub = Pulumi.Github;

return await Deployment.RunAsync(() =>
{
    var pulumi_configuration = new Config();

    var pulumi_orginization_name = pulumi_configuration.Get("pulumi_orginization_name");

    var pulumi_common_project_name = pulumi_configuration.Get("pulumi_common_project_name");
    var pulumi_common_stack_name = pulumi_configuration.Get("pulumi_common_stack_name");

    var pulumi_websites_project_name = pulumi_configuration.Get("pulumi_websites_project_name");
    var pulumi_websites_stack_name = pulumi_configuration.Get("pulumi_websites_stack_name");

    var common_stack_reference = new Pulumi.StackReference(
        "common-stack-reference",
        new()
        {
            Name = $"{pulumi_orginization_name}/{pulumi_common_project_name}/{pulumi_common_stack_name}",
        }
    );

    var websites_stack_reference = new Pulumi.StackReference(
        "websites-stack-reference",
        new()
        {
            Name = $"{pulumi_orginization_name}/{pulumi_websites_project_name}/{pulumi_websites_stack_name}",
        }
    );

    var github_project = new GitHub.Repository(
        "github-project",
        new()
        {
            Name = "websites-wpieterse-me",
            Description = "Personal resume and blog",
            Visibility = "public",
        }
    );

    var gitlab_project = new GitLab.Project(
        "gitlab-project",
        new()
        {
            Path = "wpieterse.me",
            Name = "Wynand Pieterse",
            Description = "Personal resume and blog",
            NamespaceId = websites_stack_reference.GetOutput("gitlab_group_wpieterse_websites_id").Apply(id => Int32.Parse((string)id)),
            VisibilityLevel = "public",
            PagesAccessLevel = "public",
        }
    );

    var gitlab_pages_domain_root = new GitLab.PagesDomain(
        "gitlab-pages-domain-root",
        new()
        {
            Project = gitlab_project.Id,
            Domain = "wpieterse.me",
            AutoSslEnabled = true,
        }
    );

    var domain_record_root_server = new Gcp.Dns.RecordSet(
        "domain-record-root-server",
        new()
        {
            Name = common_stack_reference.GetOutput("domain_wpieterse_me_domain_name").Apply(dnsName => $"{dnsName}"),
            Type = "A",
            Ttl = 300,
            ManagedZone = common_stack_reference.GetOutput("domain_wpieterse_me_resource_name").Apply(name => $"{name}"),
            Rrdatas = new[]
            {
                "35.185.44.232",
            }
        }
    );

    var domain_record_root_gitlab_pages_verification = new Gcp.Dns.RecordSet(
        "domain-record-root-gitlab-pages-verification",
        new()
        {
            Name = common_stack_reference.GetOutput("domain_wpieterse_me_domain_name").Apply(dnsName => $"_gitlab-pages-verification-code.{dnsName}"),
            Type = "TXT",
            Ttl = 300,
            ManagedZone = common_stack_reference.GetOutput("domain_wpieterse_me_resource_name").Apply(name => $"{name}"),
            Rrdatas = new[]
            {
                gitlab_pages_domain_root.VerificationCode.Apply(verificationCode => $"gitlab-pages-verification-code={verificationCode}")
            }
        }
    );

    var gitlab_pages_domain_www = new GitLab.PagesDomain(
        "gitlab-pages-domain-www",
        new()
        {
            Project = gitlab_project.Id,
            Domain = "www.wpieterse.me",
            AutoSslEnabled = true,
        }
    );

    var domain_record_www_server = new Gcp.Dns.RecordSet(
        "domain-record-www-server",
        new()
        {
            Name = common_stack_reference.GetOutput("domain_wpieterse_me_domain_name").Apply(dnsName => $"www.{dnsName}"),
            Type = "CNAME",
            Ttl = 300,
            ManagedZone = common_stack_reference.GetOutput("domain_wpieterse_me_resource_name").Apply(name => $"{name}"),
            Rrdatas = new[]
            {
                "wpieterse.gitlab.io.",
            }
        }
    );

    var domain_record_www_gitlab_pages_verification = new Gcp.Dns.RecordSet(
        "domain-record-www-gitlab-pages-verification",
        new()
        {
            Name = common_stack_reference.GetOutput("domain_wpieterse_me_domain_name").Apply(dnsName => $"_gitlab-pages-verification-code.www.{dnsName}"),
            Type = "TXT",
            Ttl = 300,
            ManagedZone = common_stack_reference.GetOutput("domain_wpieterse_me_resource_name").Apply(name => $"{name}"),
            Rrdatas = new[]
            {
                gitlab_pages_domain_www.VerificationCode.Apply(verificationCode => $"gitlab-pages-verification-code={verificationCode}")
            }
        }
    );
});
