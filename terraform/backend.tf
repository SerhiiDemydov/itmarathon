terraform {
  backend "s3" {
    bucket       = "epam-marathon-5"
    key          = "terraform.tfstate"
    region       = "eu-central-1"
    use_lockfile = true
    encrypt      = true
  }
}
